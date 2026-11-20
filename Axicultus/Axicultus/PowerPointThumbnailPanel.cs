using System;
using System.Windows.Forms;
using OfficeLib;

namespace Axicultus;

internal sealed class PowerPointThumbnailPanel : FlowLayoutPanel
{
    private int selectedSlide;
    private string imagePrefix;
    private int renderedWidth;

    internal ImageCanvas SlideAt(int index) => (ImageCanvas)Controls[index];

    internal void ResetSelection() => selectedSlide = 0;

    internal void SetSlideCount(int count, ToolTip toolTip, Func<int, ImageCanvas> createCanvas)
    {
        count = Math.Max(0, count);
        ResetSelection();
        SuspendLayout();
        try
        {
            while (Controls.Count > count)
            {
                var removed = SlideAt(Controls.Count - 1);
                toolTip.SetToolTip(removed, null);
                removed.Dispose();
            }
            while (Controls.Count < count)
                Controls.Add(createCanvas(Controls.Count));
        }
        finally
        {
            ResumeLayout();
        }
    }

    internal void Display(string prefix, int requestedSlide, ToolTip toolTip)
    {
        int count = Controls.Count;
        if (count == 0)
            return;

        int current = requestedSlide < 0 ? count : Math.Clamp(requestedSlide, 1, count);
        int width = Math.Max(1, (Width - 35) / 3);
        void Render(int number)
        {
            string path = SlideImageManifest.ImagePath(prefix, number);
            SlideAt(number - 1).BuildImageThumbnail(number - 1, width, width * 3 / 4, path, current, BackColor, toolTip, true);
        }

        if (selectedSlide == 0 || imagePrefix != prefix || renderedWidth != width)
        {
            SuspendLayout();
            try
            {
                for (int slide = 1; slide <= count; slide++)
                    Render(slide);
            }
            finally
            {
                ResumeLayout();
            }
        }
        else if (selectedSlide != current)
        {
            Render(selectedSlide);
            Render(current);
        }
        selectedSlide = current;
        imagePrefix = prefix;
        renderedWidth = width;
        ScrollControlIntoView(SlideAt(current - 1));
    }
}
