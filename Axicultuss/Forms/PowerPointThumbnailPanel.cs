using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OfficeLib;

namespace Axicultus;

internal sealed class PowerPointThumbnailPanel : FlowLayoutPanel
{
    private int selectedSlide;
    private string imagePrefix;
    private int renderedWidth;
    // The slide size of a deck, read once for its prefix (read again while no picture could be read).
    private string sizedPrefix;
    private Size deckSlideSize;

    internal int SelectedSlide => selectedSlide;

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
        int height = width * 3 / 4;
        void Render(int number)
        {
            string path = SlideImageManifest.ImagePath(prefix, number);
            SlideAt(number - 1).BuildImageThumbnail(number - 1, width, height, path, current, BackColor, toolTip, true);
        }

        if (selectedSlide == 0 || imagePrefix != prefix || renderedWidth != width)
        {
            // The thumbnails get the deck's slide height before their pictures load, so the rows (and the scroll
            // to the current slide) do not move when the pictures paint. Until a picture of the deck has been
            // exported, they keep the 4:3 placeholder and size themselves as their pictures load.
            if (sizedPrefix != prefix || deckSlideSize.IsEmpty)
            {
                sizedPrefix = prefix;
                deckSlideSize = DeckSlideSize(prefix, current, count);
            }
            if (!deckSlideSize.IsEmpty)
                height = ImageCanvas.PowerPointThumbnailHeight(width, deckSlideSize);
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
            SlideAt(selectedSlide - 1).UpdateSlideSelection(current);
            SlideAt(current - 1).UpdateSlideSelection(current);
        }
        selectedSlide = current;
        imagePrefix = prefix;
        renderedWidth = width;
        ScrollControlIntoView(SlideAt(current - 1));
    }

    // The slide size of a deck, from the header of one of its exported slide pictures (all are exported at the
    // deck's slide size); empty when none is readable.
    private static Size DeckSlideSize(string prefix, int current, int count)
    {
        foreach (int number in new[] { current, 1, count }.Distinct())
        {
            string path = SlideImageManifest.ImagePath(prefix, number);
            if (!File.Exists(path))
                continue;
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var image = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);
                if (image.Width > 0 && image.Height > 0)
                    return image.Size;
            }
            catch (Exception)
            {
                // Still being written, locked or unreadable (GDI+ reports a half-written JPG in several ways):
                // try another slide.
            }
        }
        return Size.Empty;
    }
}
