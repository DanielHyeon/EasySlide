using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Easislides
{
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

        private const string MediaMarker = " <#>";

        // Digits at the end, optionally followed by one bracketed mark: "136(새)", "23", "5 [B]", "7（新）".
        private static readonly Regex TrailingNumber = new Regex(
            @"^(?<title>.*?)(?<number>[0-9]+\s*(?:\([^()]*\)|\[[^\[\]]*\]|（[^（）]*）)?)$",
            RegexOptions.CultureInvariant | RegexOptions.Singleline);

        // Characters that join the trailing digits to the title: no split when one stands right before them.
        private const string NumberJoiners = ":.：．";

        private readonly ListView list;
        private readonly Func<bool> isActive;
        private readonly EraseFilter eraseFilter;
        private bool attached;

        // Note icons, one per colour, in a 32-bit image list (drawn with its alpha by ImageList_DrawEx).
        private ImageList noteImages;
        private readonly List<Bitmap> noteBitmaps = new List<Bitmap>();
        private readonly Dictionary<int, int> noteIndex = new Dictionary<int, int>();

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
        {
            text ??= "";
            media = text.EndsWith(MediaMarker, StringComparison.Ordinal);
            if (media)
            {
                text = text.Substring(0, text.Length - MediaMarker.Length);
            }
            title = text;
            number = null;
            Match m = TrailingNumber.Match(text.TrimEnd());
            if (m.Success)
            {
                string before = m.Groups["title"].Value;
                // A number right after ':' or '.' belongs to the title ("요한복음 3:16", "Song 2.5", "1.5").
                if (before.Length > 0 && NumberJoiners.IndexOf(before[before.Length - 1]) >= 0)
                {
                    return;
                }
                string head = before.TrimEnd();
                if (head.Length > 0)
                {
                    title = head;
                    number = m.Groups["number"].Value;
                }
            }
        }

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
            Color text = selected ? (hasFocus ? SystemColors.HighlightText : SystemColors.ControlText) : item.ForeColor;
            Color secondary = selected ? text : ModernUiPalette.TextSecondary;
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
                    int numberWidth = TextWidth(hdc, number, font);
                    int numberRight = bounds.Right - list.LogicalToDeviceUnits(NumberRightPadding);
                    int numberLeft = Math.Max(numberRight - numberWidth, textLeft);
                    Rectangle numberRect = Rectangle.FromLTRB(numberLeft, bounds.Top, Math.Max(numberLeft, numberRight), bounds.Bottom);
                    DrawGdiText(hdc, number, font, numberRect, secondary, numberRect.Width < numberWidth ? TitleFlags : NumberFlags);
                    titleRight = Math.Min(titleRight, numberLeft - list.LogicalToDeviceUnits(NumberGap));
                }

                int noteSize = media ? NoteSize(bounds.Height) : 0;
                int noteSpace = media ? noteSize + list.LogicalToDeviceUnits(NoteIconGap) : 0;
                int titleLimit = titleRight - noteSpace;
                if (titleLimit > textLeft)
                {
                    DrawGdiText(hdc, title, font, Rectangle.FromLTRB(textLeft, bounds.Top, titleLimit, bounds.Bottom), text, TitleFlags);
                    if (media)
                    {
                        int drawnRight = Math.Min(textLeft + TextWidth(hdc, title, font), titleLimit);
                        DrawNote(hdc, drawnRight + list.LogicalToDeviceUnits(NoteIconGap), bounds.Top + (bounds.Height - noteSize) / 2, noteSize, secondary);
                    }
                }

                if (hasFocus && item.Focused && ShowFocusCues())
                {
                    // The list's own focus rectangle, from the label to the row's right end.
                    RECT focus = new RECT { Left = label.Left, Top = bounds.Top, Right = bounds.Right, Bottom = bounds.Bottom };
                    DrawFocusRect(hdc, ref focus);
                }
            }
            finally
            {
                EndRowBuffer(target, hdc, bounds);
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

        [DllImport("comctl32.dll")]
        private static extern bool ImageList_DrawEx(IntPtr himl, int index, IntPtr hdc, int x, int y, int dx, int dy, int rgbBk, int rgbFg, int style);

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
                        ExcludeClipRect(hdc, row.Left, row.Top, row.Left + width, row.Bottom);
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

        private sealed class EraseFilter : NativeWindow
        {
            private readonly SongListPainter owner;

            public EraseFilter(SongListPainter owner)
            {
                this.owner = owner;
            }

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
                Size size = new Size(Math.Max(bounds.Width, bufferSize.Width), Math.Max(bounds.Height, bufferSize.Height));
                bufferBitmap = CreateCompatibleBitmap(target, size.Width, size.Height);
                if (bufferBitmap == IntPtr.Zero)
                {
                    return target;
                }
                bufferOldBitmap = SelectObject(bufferDC, bufferBitmap);
                bufferSize = size;
            }
            SetViewportOrgEx(bufferDC, -bounds.X, -bounds.Y, IntPtr.Zero);
            return bufferDC;
        }

        private static void EndRowBuffer(IntPtr target, IntPtr hdc, Rectangle bounds)
        {
            if (hdc != target)
            {
                const int SRCCOPY = 0x00CC0020;
                BitBlt(target, bounds.X, bounds.Y, bounds.Width, bounds.Height, hdc, bounds.X, bounds.Y, SRCCOPY);
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
        private const int DT_CALCRECT = 0x400, DT_NOCLIP = 0x100;
        private const int TitleFlags = DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX | DT_END_ELLIPSIS;
        private const int NumberFlags = DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX | DT_NOCLIP;

        private readonly Dictionary<int, IntPtr> brushes = new Dictionary<int, IntPtr>();

        // The list's font handle (WM_GETFONT): set by WinForms from list.Font, owned by WinForms.
        private IntPtr RegularFontHandle()
        {
            const int WM_GETFONT = 0x0031;
            return SendMessage(list.Handle, WM_GETFONT, IntPtr.Zero, IntPtr.Zero);
        }

        private static void DrawGdiText(IntPtr hdc, string text, IntPtr font, Rectangle rect, Color colour, int flags)
        {
            IntPtr oldFont = SelectObject(hdc, font);
            int oldMode = SetBkMode(hdc, 1); // TRANSPARENT
            int oldColour = SetTextColor(hdc, ColorTranslator.ToWin32(colour));
            RECT r = new RECT { Left = rect.Left, Top = rect.Top, Right = rect.Right, Bottom = rect.Bottom };
            DrawText(hdc, text, text.Length, ref r, flags);
            SetTextColor(hdc, oldColour);
            SetBkMode(hdc, oldMode);
            SelectObject(hdc, oldFont);
        }

        private static int TextWidth(IntPtr hdc, string text, IntPtr font)
        {
            IntPtr oldFont = SelectObject(hdc, font);
            RECT r = new RECT();
            DrawText(hdc, text, text.Length, ref r, DT_CALCRECT | DT_SINGLELINE | DT_NOPREFIX);
            SelectObject(hdc, oldFont);
            return r.Right - r.Left;
        }

        private void Fill(IntPtr hdc, Rectangle rect, Color colour)
        {
            int argb = colour.ToArgb();
            if (!brushes.TryGetValue(argb, out IntPtr brush))
            {
                brush = CreateSolidBrush(ColorTranslator.ToWin32(colour));
                brushes[argb] = brush;
            }
            RECT r = new RECT { Left = rect.Left, Top = rect.Top, Right = rect.Right, Bottom = rect.Bottom };
            FillRect(hdc, ref r, brush);
        }

        // Keyboard cues: the list draws its focus rectangle only when the window's UI state does not hide it.
        private bool ShowFocusCues()
        {
            const int WM_QUERYUISTATE = 0x0129;
            const int UISF_HIDEFOCUS = 0x1;
            return ((int)SendMessage(list.Handle, WM_QUERYUISTATE, IntPtr.Zero, IntPtr.Zero) & UISF_HIDEFOCUS) == 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int DrawText(IntPtr hdc, string text, int count, ref RECT rect, int format);

        [DllImport("user32.dll")]
        private static extern bool DrawFocusRect(IntPtr hdc, ref RECT rect);

        [DllImport("user32.dll")]
        private static extern int FillRect(IntPtr hdc, ref RECT rect, IntPtr brush);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateSolidBrush(int colour);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

        [DllImport("gdi32.dll")]
        private static extern int SetBkMode(IntPtr hdc, int mode);

        [DllImport("gdi32.dll")]
        private static extern int SetTextColor(IntPtr hdc, int colour);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr obj);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool SetViewportOrgEx(IntPtr hdc, int x, int y, IntPtr oldOrigin);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr dest, int x, int y, int width, int height, IntPtr source, int sx, int sy, int rop);

        [DllImport("gdi32.dll")]
        private static extern int SaveDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool RestoreDC(IntPtr hdc, int savedDC);

        [DllImport("gdi32.dll")]
        private static extern int ExcludeClipRect(IntPtr hdc, int left, int top, int right, int bottom);
    }
}
