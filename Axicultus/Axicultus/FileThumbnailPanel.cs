using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

// The scrollbar counts logical rows. Only visible rows own ImageCanvas controls and image resources.
internal sealed class FileThumbnailPanel : Panel
{
    private const int Columns = 3;
    private readonly VScrollBar scroll = new() { Dock = DockStyle.Right, TabStop = true };
    private readonly Dictionary<int, ImageCanvas> cells = [];
    private readonly Stack<ImageCanvas> recycled = [];
    private int renderedFirst, renderedCount, renderedWidth, renderedSelection;
    private string[] renderedPaths;
    private string[] paths = [];
    private ToolTip tip;
    private MouseEventHandler clicked;
    private EventHandler doubleClicked;
    private bool presentation, updating;
    private int selected;

    public FileThumbnailPanel()
    {
        Controls.Add(scroll);
        scroll.ValueChanged += (_, _) => Render();
    }

    internal void Display(string[] files, int selection, ToolTip toolTip, MouseEventHandler click,
        EventHandler doubleClick, bool external)
    {
        bool reset = !ReferenceEquals(paths, files);
        paths = files;
        tip = toolTip;
        clicked = click;
        doubleClicked = doubleClick;
        presentation = external;
        if (reset || selection > 0) selected = Math.Clamp(selection, 0, paths.Length);
        if (reset) scroll.Value = 0;
        Render();
        if (selection > 0 && selected > 0)
        {
            int row = (selected - 1) / Columns;
            if (row < scroll.Value || row >= scroll.Value + scroll.LargeChange)
                scroll.Value = Math.Min(row, Math.Max(0, scroll.Maximum - scroll.LargeChange + 1));
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (scroll.Visible)
            scroll.Value = Math.Clamp(scroll.Value - Math.Sign(e.Delta) * Math.Max(1, SystemInformation.MouseWheelScrollLines),
                0, Math.Max(0, scroll.Maximum - scroll.LargeChange + 1));
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        Render();
    }

    private void Render()
    {
        if (tip == null || updating || IsDisposed || Disposing) return;
        updating = true;
        SuspendLayout();
        try
        {
            int gap = LogicalToDeviceUnits(3);
            int width = Math.Max(1, (ClientSize.Width - scroll.Width) / Columns - 2 * gap);
            int height = Math.Max(1, width * 3 / 4);
            int rowHeight = height + 2 * gap;
            int totalRows = (paths.Length + Columns - 1) / Columns;
            int visibleRows = Math.Max(1, ClientSize.Height / rowHeight);
            scroll.LargeChange = visibleRows;
            scroll.Maximum = Math.Max(0, totalRows - 1);
            scroll.Value = Math.Min(scroll.Value, Math.Max(0, totalRows - visibleRows));
            scroll.Visible = totalRows > visibleRows;
            int first = scroll.Value * Columns;
            int count = Math.Min(paths.Length - first, (visibleRows + 1) * Columns);
            for (int index = renderedFirst; index < renderedFirst + renderedCount; index++)
            {
                if ((index < first || index >= first + count) && cells.Remove(index, out var offscreen))
                    recycled.Push(offscreen);
            }
            bool refresh = !ReferenceEquals(paths, renderedPaths) || renderedWidth != width || renderedSelection != selected;
            for (int i = 0; i < count; i++)
            {
                int index = first + i;
                bool retained = cells.TryGetValue(index, out var cell);
                if (!retained)
                {
                    if (!recycled.TryPop(out cell))
                    {
                        cell = new ImageCanvas { KeepThumbnailCellHeight = true };
                        cell.MouseUp += clicked;
                        if (doubleClicked != null) cell.DoubleClick += doubleClicked;
                        Controls.Add(cell);
                    }
                    cells.Add(index, cell);
                    cell.Tag = index;
                }
                cell.Location = new Point(gap + i % Columns * (width + 2 * gap), gap + i / Columns * rowHeight);
                if (!retained || refresh)
                    cell.BuildImageThumbnail(index, width, height, paths[index], selected, BackColor, tip, presentation);
                cell.Visible = true;
            }
            while (recycled.TryPop(out var unused))
            {
                tip.SetToolTip(unused, null);
                unused.Dispose();
            }
            renderedFirst = first;
            renderedCount = count;
            renderedPaths = paths;
            renderedWidth = width;
            renderedSelection = selected;
        }
        finally
        {
            ResumeLayout();
            updating = false;
        }
    }
}
