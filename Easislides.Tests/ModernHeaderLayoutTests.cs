using System;
using System.Drawing;
using System.Linq;
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
        OnStaThread(() =>
        {
            using var header = new Panel { Size = new Size(width, 33) };
            ListView title = AddHeaderControls(header, [38, 38, 48], output ? 16 : 0);
            header.Width += 48;
            header.PerformLayout();

            Assert.Equal(BorderStyle.None, title.BorderStyle);
            Assert.Equal(1, title.Left);
            Assert.Equal(title.Right, header.Controls[1].Left);
            AssertHeaderFits(header);
        });
    }

    [Theory]
    [InlineData(100, 0, false)]
    [InlineData(125, 1, false)]
    [InlineData(150, 2, false)]
    [InlineData(125, 0, true)]
    public void NestedSplittersKeepBothHeadersVisibleAtTheWindowMinimum(int scale, int initialSplit, bool startMaximized)
    {
        OnStaThread(() =>
        {
            using var form = new Form { ClientSize = new Size(initialSplit == 0 ? 220 : 900, 360) };
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            if (initialSplit == 0)
            {
                form.Location = new Point(screen.Right - form.Width, screen.Top);
            }
            var main = new SplitContainer { Dock = DockStyle.Fill, Panel2MinSize = 50 };
            var panes = new SplitContainer { Dock = DockStyle.Fill };
            var preview = new Panel { Dock = DockStyle.Top, Height = 33 };
            var output = new Panel { Dock = DockStyle.Top, Height = 33 };
            form.Controls.Add(main);
            main.Panel2.Controls.Add(panes);
            panes.Panel1.Controls.Add(preview);
            panes.Panel2.Controls.Add(output);

            int Scale(int length) => length * scale / 100;
            ListView previewTitle = AddHeaderControls(preview, [Scale(56), Scale(42), Scale(42)]);
            ListView outputTitle = AddHeaderControls(output, [Scale(40), Scale(40), Scale(56)], Scale(16));
            if (initialSplit == 1)
            {
                main.SplitterDistance = 500;
                panes.SplitterDistance = 25;
            }
            else if (initialSplit == 2)
            {
                main.SplitterDistance = 650;
                panes.SplitterDistance = panes.Width - panes.SplitterWidth - panes.Panel2MinSize;
            }
            if (startMaximized)
            {
                form.WindowState = FormWindowState.Maximized;
            }

            typeof(FrmMain).GetMethod("ApplyHeaderMinimumWidths", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, [form, main, panes, preview, output, form.Width - form.ClientSize.Width]);
            form.WindowState = FormWindowState.Normal;
            Assert.True(form.MinimumSize.Width > 220);
            if (initialSplit == 0)
            {
                Assert.Equal(screen.Right, form.Right);
            }
            AssertAllHeadersFit();

            foreach (bool rightmost in new[] { false, true })
            {
                form.Width = 900;
                main.SplitterDistance = rightmost ? main.Width : 0;
                panes.SplitterDistance = rightmost ? panes.Width : 0;
                AssertAllHeadersFit();

                foreach (int width in new[] { 820, 640, form.MinimumSize.Width + 1, 1 })
                {
                    form.Width = width;
                    AssertAllHeadersFit();
                }
                Assert.Equal(form.MinimumSize.Width, form.Width);
            }

            void AssertAllHeadersFit()
            {
                foreach (Panel header in new[] { preview, output })
                {
                    Rectangle bounds = header.ClientRectangle;
                    for (Control child = header; child.Parent is Control parent; child = parent)
                    {
                        bounds.Offset(child.Location);
                        Assert.True(parent.ClientRectangle.Contains(bounds),
                            $"Window {form.Width}: header bounds {bounds} exceed {parent.GetType().Name} {parent.ClientSize}.");
                    }
                    AssertHeaderFits(header);
                }
                Assert.True(previewTitle.Width > 0);
                Assert.True(outputTitle.Width > 0);
            }
        });
    }

    private static ListView AddHeaderControls(Panel header, int[] widths, int gapWidth = 0)
    {
        var title = new ListView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
        var first = new Button { Dock = DockStyle.Right, Width = widths[0], Text = "1" };
        var middle = new Button { Dock = DockStyle.Right, Width = widths[1], Text = "2" };
        var right = new Button { Dock = DockStyle.Right, Width = widths[2], Text = "3" };
        header.Controls.AddRange([first, middle, title, right]);
        if (gapWidth > 0)
        {
            var gap = new Panel { Dock = DockStyle.Right, Width = gapWidth };
            header.Controls.Add(gap);
            header.Controls.SetChildIndex(gap, header.Controls.GetChildIndex(right));
        }

        typeof(FrmMain).GetMethod("ApplyHeaderTint", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, [header, title, gapWidth > 0 ? Color.MistyRose : Color.AliceBlue]);
        return title;
    }

    private static void AssertHeaderFits(Panel header)
    {
        Rectangle inside = Rectangle.Inflate(header.ClientRectangle, -1, -1);
        foreach (Control item in header.Controls)
        {
            Assert.True(inside.Contains(item.Bounds), $"Header {header.ClientSize} does not contain {item.Text} {item.Bounds}.");
            Assert.Equal(inside.Top, item.Top);
            Assert.Equal(inside.Bottom, item.Bottom);
        }
        Assert.Equal(inside.Right, header.Controls.Cast<Control>().Max(item => item.Right));

        using var bitmap = new Bitmap(header.Width, header.Height);
        header.DrawToBitmap(bitmap, header.ClientRectangle);
        Color border = bitmap.GetPixel(0, 0);
        Assert.NotEqual(header.BackColor.ToArgb(), border.ToArgb());
        for (int x = 0; x < bitmap.Width; x++)
        {
            Assert.Equal(border, bitmap.GetPixel(x, 0));
            Assert.Equal(border, bitmap.GetPixel(x, bitmap.Height - 1));
        }
        for (int y = 0; y < bitmap.Height; y++)
        {
            Assert.Equal(border, bitmap.GetPixel(0, y));
            Assert.Equal(border, bitmap.GetPixel(bitmap.Width - 1, y));
        }
    }

    private static void OnStaThread(Action check)
    {
        ExceptionDispatchInfo failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                check();
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
