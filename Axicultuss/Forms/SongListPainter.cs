using static Axicultus.Interop.DesktopDrawing;
using static Axicultus.Interop.DesktopWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

/// <summary>
/// Paints the rows of the song list in the new look (design D26, task 5.1). Painting only: it never changes
/// the list's items, texts, sub-items, selection or order, and adds no input handler. When a title ends with
/// digits, optionally followed by one bracketed mark ("가나안의혼인잔치136(새)"), the part before the
/// digits is drawn on the left in the row's text colour and the digits with the mark on the right in
/// TextSecondary (the selection's text colour on a selected row); the media marker " &lt;#&gt;" becomes a
/// small note icon after the title. Everything else is drawn as the list draws it: the list's own font
/// handle and GDI DrawText, its selection colours with and without focus, its focus rectangle. Rows are
/// painted off screen and copied in one step, and the erase filter keeps the painted rows out of the
/// background erase (same technique as WorshipListPainter, design D15; the code is a minimal copy so the
/// worship list's drawing is not touched). Default drawing (DrawDefault) whenever the gate is closed, the
/// list shows the app's own number column ("Use Song Numbering"), or the folder is right-to-left.
/// </summary>
internal sealed class SongListPainter : IDisposable
{
    // Logical (96 dpi) sizes.
    private const int NumberRightPadding = 6;
    private const int NumberGap = 8;
    private const int NoteIconSize = 14;
    private const int NoteIconGap = 4;


    private readonly ListView list;
    private readonly Func<bool> isActive;
    private readonly EraseFilter eraseFilter;
    private bool attached;

    // Note icons, one per colour, in a 32-bit image list (drawn with its alpha by ImageList_DrawEx).
    private ImageList noteImages;
    private readonly List<Bitmap> noteBitmaps = [];
    private readonly Dictionary<int, int> noteIndex = [];

    public SongListPainter(ListView list, Func<bool> isActive)
    {
        this.list = list;
        this.isActive = isActive;
        eraseFilter = new EraseFilter(this);
    }

    public void Attach()
    {
        if (attached)
        {
            return;
        }
        attached = true;
        list.DrawColumnHeader += List_DrawColumnHeader;
        list.DrawItem += List_DrawItem;
        list.DrawSubItem += List_DrawSubItem;
        list.HandleCreated += List_HandleCreated;
        list.OwnerDraw = true;
        if (list.IsHandleCreated)
        {
            eraseFilter.AssignHandle(list.Handle);
        }
    }

    /// <summary>
    /// Gives the list back to the system's drawing (gate closed): handlers removed, OwnerDraw back to its
    /// designer value (false), erase filter released. Attach turns it on again; both can run any number of times.
    /// </summary>
    public void Detach()
    {
        if (!attached)
        {
            return;
        }
        attached = false;
        list.DrawColumnHeader -= List_DrawColumnHeader;
        list.DrawItem -= List_DrawItem;
        list.DrawSubItem -= List_DrawSubItem;
        list.HandleCreated -= List_HandleCreated;
        if (!list.IsDisposed)
        {
            list.OwnerDraw = false;
        }
        eraseFilter.ReleaseHandle();
    }

    /// <summary>For tests and checks: the painter's handlers are on the list.</summary>
    internal bool IsAttached => attached;

    public void Dispose()
    {
        Detach();
        foreach (IntPtr brush in brushes.Values)
        {
            DeleteObject(brush);
        }
        brushes.Clear();
        DisposeNoteImages();
        DisposeBuffer();
    }

    /// <summary>
    /// Split drawing applies: gate open, Details view, no number column of the app's own (its fifth column
    /// gets a width when "Use Song Numbering" is on), left-to-right folder. Read at every paint.
    /// </summary>
    private bool Active => isActive() && list.View == View.Details && list.RightToLeft != RightToLeft.Yes
        && !(list.Columns.Count > 4 && list.Columns[4].Width > 0);

    /// <summary>
    /// Splits a row text into title, trailing number (null when the title does not end with one) and
    /// media marker. The title keeps everything before the digits, trailing blanks removed; a text that is
    /// only a number is not split.
    /// </summary>
    internal static void SplitTitle(string text, out string title, out string number, out bool media)
        => Axicultus.Core.SongRowTitle.Split(text, out title, out number, out media);

    private void List_HandleCreated(object sender, EventArgs e)
    {
        if (eraseFilter.Handle != list.Handle)
        {
            eraseFilter.AssignHandle(list.Handle);
        }
    }

    private void List_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
        e.DrawDefault = true;
    }

    private void List_DrawItem(object sender, DrawListViewItemEventArgs e)
    {
        // Details view: the row is painted from the first column's sub-item (List_DrawSubItem).
        e.DrawDefault = !Active;
    }

    private void List_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
        if (!Active)
        {
            e.DrawDefault = true;
            return;
        }
        if (e.ColumnIndex != 0 || e.Item == null)
        {
            // The other columns have zero width here (the number column has a width only when not Active).
            return;
        }
        try
        {
            PaintRow(e.Graphics, e.Item, e.Bounds);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"SongListPainter failed: {ex.Message}");
            e.DrawDefault = true;
        }
    }

    private void PaintRow(Graphics g, ListViewItem item, Rectangle bounds)
    {
        bool hasFocus = list.Focused;
        // As the control shows it: with HideSelection the selection is not shown while unfocused.
        bool selected = item.Selected && (hasFocus || !list.HideSelection);
        Color back = selected ? (hasFocus ? SystemColors.Highlight : SystemColors.Control) : item.BackColor;
        Color textColor = selected ? (hasFocus ? SystemColors.HighlightText : SystemColors.ControlText) : item.ForeColor;
        Color secondary = selected ? textColor : ModernUiPalette.TextSecondary;
        SplitTitle(item.Text, out string title, out string number, out bool media);

        Rectangle label = item.GetBounds(ItemBoundsPortion.Label);
        IntPtr target = g.GetHdc();
        IntPtr hdc = target;
        try
        {
            hdc = BeginRowBuffer(target, bounds);

            // As the list paints a row: the row colour starts at the label.
            int rowColourLeft = Math.Max(bounds.Left, Math.Min(label.Left, bounds.Right));
            if (rowColourLeft > bounds.Left)
            {
                Fill(hdc, Rectangle.FromLTRB(bounds.Left, bounds.Top, rowColourLeft, bounds.Bottom), list.BackColor);
            }
            Fill(hdc, Rectangle.FromLTRB(rowColourLeft, bounds.Top, bounds.Right, bounds.Bottom), back);

            IntPtr font = RegularFontHandle();
            int textLeft = label.Left + TitleTextOffset;
            int titleRight = Math.Min(label.Right - TitleTextOffset, bounds.Right);
            if (number != null)
            {
                int numberWidth = GdiListText.Measure(hdc, number, font);
                int numberRight = bounds.Right - list.LogicalToDeviceUnits(NumberRightPadding);
                int numberLeft = Math.Max(numberRight - numberWidth, textLeft);
                Rectangle numberRect = Rectangle.FromLTRB(numberLeft, bounds.Top, Math.Max(numberLeft, numberRight), bounds.Bottom);
                GdiListText.Draw(hdc, number, font, numberRect, secondary, numberRect.Width < numberWidth ? TitleFlags : NumberFlags);
                titleRight = Math.Min(titleRight, numberLeft - list.LogicalToDeviceUnits(NumberGap));
            }

            int noteSize = media ? NoteSize(bounds.Height) : 0;
            int noteSpace = media ? noteSize + list.LogicalToDeviceUnits(NoteIconGap) : 0;
            int titleLimit = titleRight - noteSpace;
            if (titleLimit > textLeft)
            {
                GdiListText.Draw(hdc, title, font, Rectangle.FromLTRB(textLeft, bounds.Top, titleLimit, bounds.Bottom), textColor, TitleFlags);
                if (media)
                {
                    int drawnRight = Math.Min(textLeft + GdiListText.Measure(hdc, title, font), titleLimit);
                    DrawNote(hdc, drawnRight + list.LogicalToDeviceUnits(NoteIconGap), bounds.Top + (bounds.Height - noteSize) / 2, noteSize, secondary);
                }
            }

            if (hasFocus && item.Focused && ShowFocusCues())
            {
                // The list's own focus rectangle, from the label to the row's right end.
                var focus = Rectangle.FromLTRB(label.Left, bounds.Top, bounds.Right, bounds.Bottom);
                DrawFocusRect(hdc, focus);
            }
            EndRowBuffer(target, hdc, bounds);
        }
        finally
        {
            g.ReleaseHdc(target);
        }
    }

    // Left offset of the text inside the label rectangle, as the list draws it.
    private int TitleTextOffset => list.LogicalToDeviceUnits(2);

    private int NoteSize(int rowHeight) => Math.Max(8, Math.Min(rowHeight - 2, list.LogicalToDeviceUnits(NoteIconSize)));

    // ----- Note icon (Fluent music_note_2, regular) -----

    private void DrawNote(IntPtr hdc, int x, int y, int size, Color colour)
    {
        if (noteImages == null || noteImages.ImageSize.Width != size)
        {
            DisposeNoteImages();
            noteImages = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(size, size) };
        }
        int argb = colour.ToArgb();
        if (!noteIndex.TryGetValue(argb, out int index))
        {
            // The image list keeps the bitmap (no copy): it lives until the list is disposed.
            Bitmap bitmap = ModernIcons.Render("music_note_2", ModernIconStyle.Regular, size, colour);
            index = -1;
            if (bitmap != null)
            {
                noteBitmaps.Add(bitmap);
                noteImages.Images.Add(bitmap);
                index = noteImages.Images.Count - 1;
            }
            noteIndex[argb] = index;
        }
        if (index >= 0)
        {
            ImageList_DrawEx(noteImages.Handle, index, hdc, x, y, 0, 0, CLR_NONE, CLR_NONE, ILD_NORMAL);
        }
    }

    private void DisposeNoteImages()
    {
        noteImages?.Dispose();
        noteImages = null;
        foreach (Bitmap bitmap in noteBitmaps)
        {
            bitmap.Dispose();
        }
        noteBitmaps.Clear();
        noteIndex.Clear();
    }

    private const int ILD_NORMAL = 0x0;
    private const int CLR_NONE = unchecked((int)0xFFFFFFFF);

    // ----- No background erase under painted rows (see WorshipListPainter.EraseFilter) -----

    private int ExcludeRowsFromErase(IntPtr hdc)
    {
        // Never throws: EraseFilter.WndProc must reach base.WndProc on every path.
        int saved = 0;
        try
        {
            if (hdc == IntPtr.Zero || !attached || !Active || list.Items.Count == 0 || list.Columns.Count == 0)
            {
                return 0;
            }
            saved = SaveDC(hdc);
            if (saved == 0)
            {
                return 0;
            }
            int width = list.Columns[0].Width;
            int clientHeight = list.ClientSize.Height;
            ListViewItem top = list.TopItem;
            for (int i = top?.Index ?? 0; i < list.Items.Count; i++)
            {
                // While the list is being refilled or cleared the control can count rows it no longer has an
                // item for (Items[i] is null): nothing more is left out, the erase covers the rest.
                ListViewItem item = list.Items[i];
                if (item == null)
                {
                    break;
                }
                Rectangle row = item.Bounds;
                if (row.Top >= clientHeight)
                {
                    break;
                }
                if (row.Bottom > 0)
                {
                    if (ExcludeClipRect(hdc, row.Left, row.Top, row.Left + width, row.Bottom) == 0)
                        throw new InvalidOperationException("Cannot exclude the row from background erase.");
                }
            }
            return saved;
        }
        catch (Exception ex)
        {
            if (saved != 0)
            {
                RestoreDC(hdc, saved);
            }
            Trace.WriteLine($"SongListPainter erase filter failed: {ex.Message}");
            return 0;
        }
    }

    private sealed class EraseFilter(SongListPainter owner) : NativeWindow
    {
        private readonly SongListPainter owner = owner;

        protected override void WndProc(ref Message m)
        {
            const int WM_ERASEBKGND = 0x0014;
            if (m.Msg != WM_ERASEBKGND)
            {
                base.WndProc(ref m);
                return;
            }
            int saved = owner.ExcludeRowsFromErase(m.WParam);
            try
            {
                base.WndProc(ref m);
            }
            finally
            {
                if (saved != 0)
                {
                    RestoreDC(m.WParam, saved);
                }
            }
        }
    }

    // ----- Row buffer: one off-screen bitmap, reused for every row (see WorshipListPainter) -----

    private IntPtr bufferDC;
    private IntPtr bufferBitmap;
    private IntPtr bufferOldBitmap;
    private Size bufferSize;

    private IntPtr BeginRowBuffer(IntPtr target, Rectangle bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return target;
        }
        if (bufferDC == IntPtr.Zero)
        {
            bufferDC = CreateCompatibleDC(target);
            if (bufferDC == IntPtr.Zero)
            {
                return target;
            }
        }
        if (bufferBitmap == IntPtr.Zero || bufferSize.Width < bounds.Width || bufferSize.Height < bounds.Height)
        {
            ReleaseBufferBitmap();
            Size size = new(Math.Max(bounds.Width, bufferSize.Width), Math.Max(bounds.Height, bufferSize.Height));
            bufferBitmap = CreateCompatibleBitmap(target, size.Width, size.Height);
            if (bufferBitmap == IntPtr.Zero)
            {
                return target;
            }
            bufferOldBitmap = SelectObject(bufferDC, bufferBitmap);
            if (bufferOldBitmap == IntPtr.Zero)
            {
                DeleteObject(bufferBitmap);
                bufferBitmap = IntPtr.Zero;
                return target;
            }
            bufferSize = size;
        }
        return SetViewportOrigin(bufferDC, -bounds.X, -bounds.Y) ? bufferDC : target;
    }

    private static void EndRowBuffer(IntPtr target, IntPtr hdc, Rectangle bounds)
    {
        if (hdc != target)
        {
            const int SRCCOPY = 0x00CC0020;
            if (!BitBlt(target, bounds.X, bounds.Y, bounds.Width, bounds.Height, hdc, bounds.X, bounds.Y, SRCCOPY))
                throw new InvalidOperationException("Cannot copy the list row to the window.");
        }
    }

    private void ReleaseBufferBitmap()
    {
        if (bufferBitmap != IntPtr.Zero)
        {
            SelectObject(bufferDC, bufferOldBitmap);
            DeleteObject(bufferBitmap);
            bufferBitmap = IntPtr.Zero;
            bufferOldBitmap = IntPtr.Zero;
            bufferSize = Size.Empty;
        }
    }

    private void DisposeBuffer()
    {
        ReleaseBufferBitmap();
        if (bufferDC != IntPtr.Zero)
        {
            DeleteDC(bufferDC);
            bufferDC = IntPtr.Zero;
        }
    }

    // ----- GDI drawing with the list's own font handle -----

    private const int DT_SINGLELINE = 0x20, DT_VCENTER = 0x4, DT_NOPREFIX = 0x800, DT_END_ELLIPSIS = 0x8000;
    private const int DT_NOCLIP = 0x100;
    private const int TitleFlags = DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX | DT_END_ELLIPSIS;
    private const int NumberFlags = DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX | DT_NOCLIP;

    private readonly Dictionary<int, IntPtr> brushes = [];

    // The list's font handle (WM_GETFONT): set by WinForms from list.Font, owned by WinForms.
    private IntPtr RegularFontHandle()
    {
        const int WM_GETFONT = 0x0031;
        return SendMessage(list.Handle, WM_GETFONT, IntPtr.Zero, IntPtr.Zero);
    }

    private void Fill(IntPtr hdc, Rectangle rect, Color colour)
    {
        int argb = colour.ToArgb();
        if (!brushes.TryGetValue(argb, out IntPtr brush))
        {
            brush = CreateSolidBrush(ColorTranslator.ToWin32(colour));
            if (brush == IntPtr.Zero) throw new InvalidOperationException("Cannot create the list background brush.");
            brushes[argb] = brush;
        }

        if (rect.Width > 0 && rect.Height > 0 && FillRect(hdc, rect, brush) == 0)
            throw new InvalidOperationException("Cannot fill the list background.");
    }

    // Keyboard cues: the list draws its focus rectangle only when the window's UI state does not hide it.
    private bool ShowFocusCues()
    {
        const int WM_QUERYUISTATE = 0x0129;
        const int UISF_HIDEFOCUS = 0x1;
        return ((int)SendMessage(list.Handle, WM_QUERYUISTATE, IntPtr.Zero, IntPtr.Zero) & UISF_HIDEFOCUS) == 0;
    }

}
