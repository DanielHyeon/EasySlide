using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Axicultus.Util;

namespace Axicultus;

internal sealed class RotationTimingPanel : Panel
{
    private readonly record struct TimingRow(string Label, int Verse, int Seconds);
    private readonly List<TimingRow> rows = [];
    private readonly List<(Label Label, DateTimePicker Picker)> editors = [];
    private readonly VScrollBar scroll = new() { Dock = DockStyle.Right };
    private bool binding;
    internal static readonly DateTime StartTime = new(2005, 1, 1);
    private int RowHeight => Font.Height + LogicalToDeviceUnits(14);
    private int PageSize => Math.Max(1, ClientSize.Height / RowHeight);

    public RotationTimingPanel()
    {
        scroll.ValueChanged += (_, _) => BindEditors();
        Controls.Add(scroll);
    }

    internal void UpdateRows(ListView order, int[] screenCounts, string savedTimes, bool load, bool reset)
    {
        var times = savedTimes.AsSpan().Split(';');
        int index = 0;
        foreach (ListViewItem item in order.Items)
        {
            int verse = DataUtil.StringToInt(item.SubItems[1].Text);
            if ((uint)verse >= (uint)screenCounts.Length)
                continue;
            for (int screen = 0; screen < screenCounts[verse]; screen++, index++)
            {
                int seconds = 0;
                if (index > 0 && !reset)
                {
                    if (!load && index < rows.Count)
                        seconds = rows[index].Seconds;
                    else if (load && times.MoveNext())
                        seconds = int.TryParse(savedTimes.AsSpan()[times.Current], out int parsed) ? parsed : 0;
                }
                var row = new TimingRow(screen == 0 ? item.Text : " - ", screen == 0 ? verse : 151, Math.Max(0, seconds));
                if (index == rows.Count)
                    rows.Add(row);
                else
                    rows[index] = row;
            }
        }
        rows.RemoveRange(index, rows.Count - index);
        if (load || reset)
            scroll.Value = 0;
        BindEditors();
    }

    internal void AddPosition(int seconds)
    {
        for (int index = 1; index < rows.Count; index++)
        {
            if (rows[index].Seconds != 0)
                continue;
            rows[index] = rows[index] with { Seconds = Math.Max(0, seconds) };
            BindEditors();
            return;
        }
    }

    internal void ClearPositions()
    {
        for (int index = 0; index < rows.Count; index++)
            rows[index] = rows[index] with { Seconds = 0 };
        BindEditors();
    }

    internal string Serialize(int style, int gap, int total, ListView order)
    {
        if (style == 1 && gap == 0 && total == 0 && order.Items.Count == 0)
            return "";
        var result = new StringBuilder().Append(style).Append(';').Append(gap).Append(';').Append(total).Append(';');
        foreach (ListViewItem item in order.Items)
            result.Append(DataUtil.StringToInt(item.SubItems[1].Text)).Append(';');

        List<int> times = [];
        foreach (var row in rows)
            if (row.Seconds > 0)
                times.Add(row.Seconds);
        if (times.Count > 0)
        {
            times.Sort();
            result.Append('»');
            foreach (int seconds in times)
                result.Append(seconds).Append(';');
        }
        return result.ToString();
    }

    private void BindEditors()
    {
        if (binding || scroll is null || IsDisposed || Disposing)
            return;
        binding = true;
        SuspendLayout();
        try
        {
            int page = PageSize;
            int first = Math.Min(scroll.Value, Math.Max(0, rows.Count - page));
            scroll.Value = first;
            scroll.Maximum = Math.Max(0, rows.Count - 1);
            scroll.LargeChange = page;
            bool canScroll = rows.Count > page;
            scroll.Visible = canScroll;
            int visible = Math.Min(rows.Count - first, (ClientSize.Height + RowHeight - 1) / RowHeight);
            while (editors.Count > visible)
            {
                var editor = editors[^1];
                editors.RemoveAt(editors.Count - 1);
                editor.Label.Dispose();
                editor.Picker.Dispose();
            }
            int margin = LogicalToDeviceUnits(3);
            int available = Math.Max(0, ClientSize.Width - (canScroll ? scroll.Width : 0) - 3 * margin);
            int pickerWidth = Math.Min(available, Math.Max(LogicalToDeviceUnits(60), TextRenderer.MeasureText("00:00", Font).Width + SystemInformation.VerticalScrollBarWidth + margin));
            int labelWidth = Math.Clamp(available - pickerWidth, 0, LogicalToDeviceUnits(60));
            for (int slot = 0; slot < visible; slot++)
            {
                if (slot == editors.Count)
                    AddEditor(slot);
                var (label, picker) = editors[slot];
                int index = first + slot;
                var row = rows[index];
                int top = slot * RowHeight + margin;
                label.SetBounds(margin, top, labelWidth, picker.Height);
                picker.SetBounds(2 * margin + labelWidth, top, pickerWidth, picker.Height);
                label.Text = row.Label;
                picker.AccessibleName = $"{index + 1} {row.Label}";
                picker.Tag = row.Verse.ToString();
                picker.Enabled = index > 0;
                picker.Value = StartTime.AddSeconds(row.Seconds);
            }
        }
        finally
        {
            ResumeLayout();
            binding = false;
        }
    }

    private void AddEditor(int slot)
    {
        var label = new Label { TextAlign = ContentAlignment.MiddleLeft };
        var picker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "mm:ss",
            ShowUpDown = true,
            MinDate = StartTime,
            TabIndex = slot + 1
        };
        picker.ValueChanged += (_, _) =>
        {
            int index = scroll.Value + slot;
            if (!binding && index > 0 && index < rows.Count)
                rows[index] = rows[index] with { Seconds = (int)(picker.Value - StartTime).TotalSeconds };
        };
        editors.Add((label, picker));
        Controls.Add(label);
        Controls.Add(picker);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        BindEditors();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        BindEditors();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        int lines = SystemInformation.MouseWheelScrollLines;
        int step = lines < 0 ? PageSize : lines;
        scroll.Value = Math.Clamp(scroll.Value - Math.Sign(e.Delta) * step, 0, Math.Max(0, rows.Count - PageSize));
    }

    protected override bool ProcessDialogKey(Keys keyData)
    {
        if (keyData is Keys.Tab or (Keys.Shift | Keys.Tab))
        {
            bool backwards = keyData.HasFlag(Keys.Shift);
            int slot = editors.FindIndex(editor => editor.Picker.Focused);
            if (slot >= 0 && (backwards ? slot == 0 : slot == Math.Min(editors.Count, PageSize) - 1))
            {
                int next = scroll.Value + (backwards ? -1 : 1);
                if (next >= (backwards ? 1 : 0) && next <= Math.Max(0, rows.Count - PageSize))
                {
                    scroll.Value = next;
                    return true;
                }
            }
        }
        return base.ProcessDialogKey(keyData);
    }
}
