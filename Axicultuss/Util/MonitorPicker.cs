using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Axicultus.Core;
using Axicultus.Localization;

namespace Axicultus.Util;

internal enum MonitorPurpose { Output, Lyrics, Media }

internal static class MonitorPicker
{
    private sealed record Choice(string Id, string Label)
    {
        public override string ToString() => Label;
    }

    internal static string SelectedId(ComboBox picker) => (picker.SelectedItem as Choice)?.Id;

    internal static void Populate(ComboBox picker, IReadOnlyList<MonitorDevice> displays, MonitorPurpose purpose, string savedId)
    {
        var choices = new List<Choice>();
        if (purpose == MonitorPurpose.Media)
            choices.Add(new Choice("", UiText.Get(nameof(MonitorPicker), "DefaultOutputMonitor")));
        if (purpose == MonitorPurpose.Output)
            choices.Add(new Choice(MonitorLayout.Auto, UiText.Get(nameof(MonitorPicker), "Auto")));

        foreach (var display in displays.OrderByDescending(d => d.IsPrimary)
                     .ThenBy(d => d.Bounds.Top).ThenBy(d => d.Bounds.Left).ThenBy(d => d.Name, StringComparer.CurrentCulture))
        {
            if (purpose == MonitorPurpose.Lyrics && display.IsPrimary)
            {
                if (string.Equals(display.Name, savedId, StringComparison.OrdinalIgnoreCase))
                    choices.Add(new Choice(display.Name, UiText.Format(nameof(MonitorPicker), "NotForLyrics", display.Label)));
            }
            else
                choices.Add(new Choice(display.Name, display.Label));
        }
        if (purpose != MonitorPurpose.Media)
            choices.Add(new Choice(MonitorLayout.None, UiText.Get(nameof(MonitorPicker), "None")));

        var selected = choices.Find(c => string.Equals(c.Id, savedId, StringComparison.OrdinalIgnoreCase));
        if (selected is null && !string.IsNullOrWhiteSpace(savedId))
        {
            selected = new Choice(savedId, MonitorLabel.Disconnected(savedId));
            choices.Add(selected);
        }
        picker.BeginUpdate();
        try
        {
            picker.Items.Clear();
            picker.Items.AddRange([.. choices]);
            picker.SelectedItem = selected ?? (purpose == MonitorPurpose.Media ? choices[0] : null);
            int labelWidth = choices.Max(c => TextRenderer.MeasureText(c.Label, picker.Font).Width);
            picker.DropDownWidth = Math.Max(picker.Width, labelWidth + SystemInformation.VerticalScrollBarWidth);
        }
        finally
        {
            picker.EndUpdate();
        }
    }
}
