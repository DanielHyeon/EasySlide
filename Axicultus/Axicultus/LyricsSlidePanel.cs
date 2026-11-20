using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

// The scroll range covers the whole song. RichEdit handles are only needed for visible slides.
internal sealed class LyricsSlidePanel : Panel
{
    private readonly VScrollBar scroll = new() { Dock = DockStyle.Right, TabStop = false };
    private readonly List<RichTextBox> boxes = [];
    private string[] texts = [];
    private int[] heights = [];
    // Fenwick tree of row heights: map slide numbers to pixels and refine wrapped heights in O(log n).
    private int[] heightSums = [];
    private Font lyricsFont;
    private Font textFont;
    private bool lines, placeholder, binding;
    private int selectedSlide;
    private Color selectedText, selectedBack;

    internal event MouseEventHandler SlideMouseUp;
    internal event KeyEventHandler SlideKeyUp;

    public LyricsSlidePanel()
    {
        scroll.ValueChanged += (_, _) => BindVisible();
        Controls.Add(scroll);
    }

    internal Font PrepareFont(float size)
    {
        if (lyricsFont != null && lyricsFont.Name == Font.Name && lyricsFont.Size == size && lyricsFont.Style == Font.Style)
            return lyricsFont;
        var previous = lyricsFont;
        lyricsFont = new Font(Font.Name, size, Font.Style);
        textFont?.Dispose();
        textFont = new Font("Microsoft Sans Serif", size, Font.Style);
        foreach (var box in boxes)
        {
            box.Font = lyricsFont;
            box.Tag = null;
        }
        previous?.Dispose();
        return lyricsFont;
    }

    internal void SetSlides(string[] slides, bool borders, bool blank = false)
    {
        texts = slides;
        lines = borders;
        placeholder = blank;
        selectedSlide = 0;
        InitializeHeights();
        SetScroll(0);
        BindVisible();
    }

    private void InitializeHeights()
    {
        foreach (var box in boxes)
            box.Tag = null;
        if (heights.Length != texts.Length)
        {
            heights = new int[texts.Length];
            heightSums = new int[texts.Length + 1];
        }
        else
            Array.Clear(heightSums);
        for (int i = 0; i < texts.Length; i++)
        {
            heights[i] = texts[i].Length == 0 ? (placeholder ? lyricsFont.Height : 5)
                : (2 + texts[i].AsSpan().Count('\n')) * lyricsFont.Height;
            int node = i + 1;
            heightSums[node] += heights[i];
            int parent = node + (node & -node);
            if (parent < heightSums.Length)
                heightSums[parent] += heightSums[node];
        }
    }

    private int TopOf(int index)
    {
        int top = 0;
        for (; index > 0; index -= index & -index)
            top += heightSums[index];
        return top;
    }

    private void SetHeight(int index, int height)
    {
        int delta = height - heights[index];
        if (delta == 0)
            return;
        heights[index] = height;
        for (int node = index + 1; node < heightSums.Length; node += node & -node)
            heightSums[node] += delta;
    }

    private int FirstVisible(int pixel)
    {
        int index = 0;
        for (int step = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)texts.Length); step > 0; step >>= 1)
        {
            int next = index + step;
            if (next < heightSums.Length && heightSums[next] <= pixel)
            {
                pixel -= heightSums[next];
                index = next;
            }
        }
        return Math.Min(index, texts.Length - 1);
    }

    private int PageHeight => Math.Max(ClientSize.Height, 1);
    private int MaxScroll => Math.Max(0, TopOf(texts.Length) - PageHeight);
    private void SetScroll(int value) => scroll.Value = Math.Clamp(value, 0, Math.Min(scroll.Maximum, MaxScroll));

    private int UpdateScrollRange()
    {
        int totalHeight = TopOf(texts.Length);
        scroll.Maximum = Math.Max(0, totalHeight - 1);
        scroll.LargeChange = PageHeight;
        scroll.SmallChange = lyricsFont?.Height ?? Font.Height;
        SetScroll(scroll.Value);
        bool needsScroll = totalHeight > PageHeight;
        scroll.Visible = needsScroll;
        return Math.Max(1, ClientSize.Width - (needsScroll ? scroll.Width : 0));
    }

    private void BindVisible()
    {
        if (binding || lyricsFont == null || IsDisposed || Disposing)
            return;
        binding = true;
        SuspendLayout();
        try
        {
            // Measuring a wrapped row can change the scrollbar width or clamp the last page.
            // In that case bind once more against the new viewport.
            int width, origin, nextWidth;
            bool keepBottom = scroll.Value > 0 && scroll.Value == MaxScroll;
            do
            {
                width = UpdateScrollRange();
                origin = scroll.Value;
                int index = texts.Length == 0 ? 0 : FirstVisible(origin);
                int y = TopOf(index) - origin;
                int slot = 0;
                while (index < texts.Length && y < ClientSize.Height)
                {
                    if (boxes.Count == slot)
                        AddBox();
                    var box = boxes[slot++];
                    bool changed = box.Tag is not int slide || slide != index + 1;
                    bool resized = box.Width != width;
                    box.SetBounds(0, y, width, heights[index]);
                    if (changed)
                    {
                        box.Tag = index + 1;
                        box.BorderStyle = lines ? BorderStyle.FixedSingle : BorderStyle.None;
                        box.Text = texts[index];
                        box.SelectAll();
                        box.SelectionFont = textFont;
                        box.Select(0, 0);
                    }
                    if ((changed || resized) && box.TextLength > 0)
                    {
                        int measured = box.GetPositionFromCharIndex(box.TextLength - 1).Y + 2 * lyricsFont.Height;
                        SetHeight(index, Math.Max(lyricsFont.Height, measured));
                        box.Height = heights[index];
                    }
                    ColourBox(box, index + 1);
                    y += heights[index++];
                }
                while (boxes.Count > slot)
                {
                    var unused = boxes[^1];
                    boxes.RemoveAt(boxes.Count - 1);
                    unused.Dispose();
                }
                nextWidth = UpdateScrollRange();
                if (keepBottom)
                    SetScroll(MaxScroll);
            } while (nextWidth != width || scroll.Value != origin);
        }
        finally
        {
            ResumeLayout();
            binding = false;
        }
    }

    private void AddBox()
    {
        var box = new RichTextBox
        {
            Name = Name == "flowLayoutOutputLyrics" ? "Output" : "Preview",
            ReadOnly = true, ScrollBars = RichTextBoxScrollBars.None, Font = lyricsFont
        };
        box.MouseUp += (sender, e) => SlideMouseUp?.Invoke(sender, e);
        box.KeyUp += (sender, e) => SlideKeyUp?.Invoke(sender, e);
        box.MouseWheel += (_, e) => ScrollWheel(e);
        boxes.Add(box);
        Controls.Add(box);
    }

    private void ColourBox(RichTextBox box, int slide)
    {
        bool selected = slide == selectedSlide && !placeholder;
        Color foreground = selected ? selectedText : Gf.NormalTextColour;
        Color background = selected ? selectedBack : BackColor;
        if (box.ForeColor != foreground) box.ForeColor = foreground;
        if (box.BackColor != background) box.BackColor = background;
    }

    internal void Highlight(int slide, Color foreground, Color background, bool navigate = false, bool alignTop = false)
    {
        selectedSlide = slide;
        selectedText = foreground;
        selectedBack = background;
        if (navigate && slide > 0 && slide <= texts.Length)
        {
            int index = slide - 1;
            int top = TopOf(index);
            if (alignTop || top < scroll.Value || heights[index] >= PageHeight)
                SetScroll(top);
            else if (top + heights[index] > scroll.Value + PageHeight)
                SetScroll(top + heights[index] - PageHeight);
            // Neighbouring slides may have wrapped to more lines on their first visit.
            // Finish the jump using the updated heights if they pushed the target out of view.
            if (TopOf(index) >= scroll.Value + PageHeight || TopOf(index + 1) <= scroll.Value)
                SetScroll(TopOf(index));
        }
        RichTextBox focusTarget = null;
        foreach (var box in boxes)
        {
            if (box.Tag is not int number)
                continue;
            ColourBox(box, number);
            if (navigate && number == slide)
                focusTarget = box;
        }
        focusTarget?.Focus();
    }

    private void ScrollWheel(MouseEventArgs e)
    {
        SetScroll(scroll.Value - e.Delta / 3);
        if (e is HandledMouseEventArgs handled) handled.Handled = true;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        ScrollWheel(e);
        base.OnMouseWheel(e);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        BindVisible();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            lyricsFont?.Dispose();
            textFont?.Dispose();
        }
    }
}
