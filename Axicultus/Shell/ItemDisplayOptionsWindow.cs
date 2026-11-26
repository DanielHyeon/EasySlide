using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Shell;

internal sealed class ItemDisplayOptionsWindow : Window
{
    private readonly Dictionary<int, string> _original;
    private readonly List<Action<Dictionary<int, string>>> _save = [];
    private readonly TextBlock _error = new() { TextWrapping = TextWrapping.Wrap };
    private readonly LiveOutputRenderSettings _defaults;
    private readonly bool _showNotations;
    private readonly SongPageLayout? _layout;
    internal string FormatData { get; private set; }

    internal ItemDisplayOptionsWindow(string? format, LiveOutputRenderSettings defaults, bool showNotations, SongPageLayout? layoutDefaults = null)
    {
        _defaults = defaults;
        _layout = layoutDefaults;
        _showNotations = showNotations;
        FormatData = format ?? "";
        _original = ItemFormatFields.Parse(format);
        Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new Uri("/Axicultus;component/Controls/OptionsLayout.xaml", UriKind.Relative) });
        Title = UiText.Get("ItemDisplayOptionsWindow", "Title");
        Width = 540; Height = 590; MinWidth = 390; MinHeight = 360;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var layout = new DockPanel { Margin = new Thickness(12) };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var ok = new Button { Content = UiText.Get("ItemDisplayOptionsWindow", "OK"), IsDefault = true, MinWidth = 68 };
        ok.Click += Save;
        buttons.Children.Add(ok);
        buttons.Children.Add(new Button { Content = UiText.Get("ItemDisplayOptionsWindow", "Cancel"), IsCancel = true, MinWidth = 68, Margin = new Thickness(4,0,0,0) });
        DockPanel.SetDock(buttons, Dock.Bottom); layout.Children.Add(buttons);
        DockPanel.SetDock(_error, Dock.Bottom); layout.Children.Add(_error);
        var tabs = new TabControl(); layout.Children.Add(tabs); Content = layout;
        static string T(string key) => UiText.Get("ItemDisplayOptionsWindow", key);
        var common = AddTab(tabs, UiText.Get("ItemDisplayOptionsWindow", "TabDisplay"));
        Choice(common, 25, UiText.Get("ItemDisplayOptionsWindow", "DisplayLanguage"), [T("MainLanguage"), T("SecondLanguage"), T("BothLanguages")]);
        Choice(common, 21, UiText.Get("ItemDisplayOptionsWindow", "SongTitle"), [T("Hide"), T("AllScreens"), T("FirstScreen")]);
        Choice(common, 23, UiText.Get("ItemDisplayOptionsWindow", "TitleAlignment"), [T("FollowMain"), T("FollowSecond"), T("Left"), T("Center"), T("Right")]);
        Flags(common, 22, [(2, T("Shadow")), (4, T("Chords")), (16, T("Interlace")), (32, T("Outline")), (64, T("HidePanel"))]);
        Field(common, 71, UiText.Get("ItemDisplayOptionsWindow", "Transpose"), 0, 11);
        int primarySize = _layout?.FontPoints ?? (int)Math.Round(defaults.LyricsMonitorFontSize * .75);
        int secondarySize = _layout?.Integer("Size1") ?? (defaults.LyricsMonitorFontSize2 > 0 ?
            (int)Math.Round(defaults.LyricsMonitorFontSize2 * .75) : primarySize);
        for (int region = 0; region < 2; region++)
        {
            var pane = AddTab(tabs, UiText.Get("ItemDisplayOptionsWindow", region == 0 ? "MainLanguage" : "SecondLanguage"));
            Field(pane, 43 + region, UiText.Get("ItemDisplayOptionsWindow", "Font"));
            Field(pane, 47 + region, UiText.Get("ItemDisplayOptionsWindow", "FontSize"), 6, 100, inherited: region == 0 ? primarySize : secondarySize);
            Field(pane, 29 + region, UiText.Format("ItemDisplayOptionsWindow", "TextColor", "#AARRGGBB"), color: true);
            Choice(pane, 31 + region, UiText.Get("ItemDisplayOptionsWindow", "HorizontalAlignment"), [T("Left"), T("Center"), T("Right")], first: 1);
            Field(pane, 45 + region, UiText.Get("ItemDisplayOptionsWindow", "VerticalPosition"), 0, 100, inherited: region == 0 ? _layout?.TopPercent ?? 0 : _layout?.Integer("Vpos1") ?? 50);
            Flags(pane, 41 + region, [(1, T("Bold")), (2, T("Italic")), (4, T("Underline")), (16, T("ChorusItalic"))]);
        }
    }

    private static StackPanel AddTab(TabControl tabs, string title)
    {
        var content = new StackPanel { Margin = new Thickness(8) };
        tabs.Items.Add(new TabItem { Header = title, Content = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } });
        return content;
    }

    private static void AddInput(Panel panel, string caption, Control control)
    {
        var label = new TextBlock { Text = caption, Margin = new Thickness(0,8,0,2) };
        AutomationProperties.SetLabeledBy(control, label);
        panel.Children.Add(label); panel.Children.Add(control);
    }

    private void Field(Panel pane, int key, string caption, int? min = null, int? max = null, bool color = false, int? inherited = null)
    {
        string initial = _original.GetValueOrDefault(key, "");
        if (color && int.TryParse(initial, out int argb)) initial = $"#{unchecked((uint)argb):X8}";
        TextBox input;
        if (inherited is int defaultValue && min is int minimum && max is int maximum)
        {
            if (initial.Length == 0) initial = defaultValue.ToString(CultureInfo.CurrentCulture);
            input = new OptionNumberBox { Minimum = minimum, Maximum = maximum,
                ToolTip = UiText.Format("ItemDisplayOptionsWindow", "DefaultTip", defaultValue) };
        }
        else input = new TextBox();
        input.Text = initial;
        AddInput(pane, caption, input);
        _save.Add(fields =>
        {
            string value = input.Text.Trim();
            if (value == initial) return;
            if (value.Length == 0) { fields.Remove(key); return; }
            if (color && value.StartsWith('#') && value.Length == 9 && uint.TryParse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hex))
                value = unchecked((int)hex).ToString(CultureInfo.InvariantCulture);
            if (min.HasValue || color)
            {
                if (!int.TryParse(value, out int n) || n < min || n > max) throw new FormatException(UiText.Format("ItemDisplayOptionsWindow", "CheckValue", caption));
            }
            if (value.Contains('>')) throw new FormatException(UiText.Format("ItemDisplayOptionsWindow", "NoGreaterThan", caption));
            fields[key] = value;
        });
    }

    private void Choice(Panel pane, int key, string caption, string[] choices, int first = 0)
    {
        int index = int.TryParse(_original.GetValueOrDefault(key), out int n) && n >= first && n < first + choices.Length ? n - first + 1 : 0;
        var input = new ComboBox { ItemsSource = choices.Prepend(UiText.Get("ItemDisplayOptionsWindow", "FollowDefault")), SelectedIndex = index };
        AddInput(pane, caption, input);
        _save.Add(fields =>
        {
            if (input.SelectedIndex == index) return;
            if (input.SelectedIndex <= 0) fields.Remove(key);
            else fields[key] = (input.SelectedIndex - 1 + first).ToString(CultureInfo.InvariantCulture);
        });
    }

    private void Flags(Panel pane, int key, (int Mask, string Label)[] flags)
    {
        bool existed = int.TryParse(_original.GetValueOrDefault(key), out int original);
        if (!existed)
        {
            original = key == 22 ? (_defaults.LyricsMonitorShadow ? 2 : 0) | (_defaults.LyricsMonitorInterlace ? 16 : 0)
                | (_defaults.ShowLyricsOutline ? 32 : 0) | (_showNotations ? 4 : 0)
                : (_defaults.LyricsMonitorBold ? 1 : 0) | (_defaults.LyricsMonitorItalic ? 2 : 0) | (_defaults.LyricsMonitorUnderline ? 4 : 0);
        }
        if (!existed && key != 22 && _layout is not null)
            original = key == 41 ? _layout.FontStyleBits : _layout.Integer("BIU1") ?? original;
        var previousStyle = SongFormatData.Parse(FormatData);
        if (key is 41 or 42 && previousStyle?.RegionStyle(key == 42) is int regionStyle)
            original = regionStyle;
        var row = new WrapPanel { Margin = new Thickness(0,8,0,4) }; pane.Children.Add(row);
        foreach (var (mask, label) in flags)
        {
            bool initial = (original & mask) != 0;
            var toggle = new CheckBox { Content = label, IsChecked = initial, Margin = new Thickness(0,0,8,0) }; row.Children.Add(toggle);
            _save.Add(fields =>
            {
                if (toggle.IsChecked == initial) return;
                if (key is 41 or 42 && !fields.ContainsKey(42) && int.TryParse(fields.GetValueOrDefault(41), out int packed) && (packed & 56) != 0)
                {
                    fields[41] = (packed & 7).ToString(CultureInfo.InvariantCulture);
                    fields[42] = ((packed >> 3) & 7).ToString(CultureInfo.InvariantCulture);
                }
                if (!int.TryParse(fields.GetValueOrDefault(key), out int current)) current = original;
                if (key == 41 && mask == 16 && !fields.ContainsKey(42))
                    fields[42] = (_layout?.Integer("BIU1") ?? (original >> 3) & 7).ToString(CultureInfo.InvariantCulture);
                fields[key] = (toggle.IsChecked == true ? current | mask : current & ~mask).ToString(CultureInfo.InvariantCulture);
            });
        }
    }

    private void Save(object sender, RoutedEventArgs args)
    {
        var fields = new Dictionary<int, string>(_original);
        try
        {
            foreach (var apply in _save) apply(fields);
            if (int.TryParse(fields.GetValueOrDefault(45), out int top) && int.TryParse(fields.GetValueOrDefault(46), out int second) && second < top)
                throw new FormatException(UiText.Get("ItemDisplayOptionsWindow", "SecondBelowMain"));
            if (!fields.SequenceEqual(_original)) FormatData = ItemFormatFields.Encode(fields);
            DialogResult = true;
        }
        catch (FormatException ex) { _error.Text = ex.Message; }
    }
}
