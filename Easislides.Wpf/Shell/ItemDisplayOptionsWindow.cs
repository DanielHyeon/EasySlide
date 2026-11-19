using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Easislides.Wpf.Library;
using Easislides.Wpf.Rendering;

namespace Easislides.Wpf.Shell;

internal sealed class ItemDisplayOptionsWindow : Window
{
    private readonly Dictionary<int, string> _original;
    private readonly List<Action<Dictionary<int, string>>> _save = [];
    private readonly TextBlock _error = new() { TextWrapping = TextWrapping.Wrap };
    private readonly LiveOutputRenderSettings _defaults;
    private readonly bool _showNotations;
    internal string FormatData { get; private set; }

    internal ItemDisplayOptionsWindow(string? format, LiveOutputRenderSettings defaults, bool showNotations)
    {
        _defaults = defaults;
        _showNotations = showNotations;
        FormatData = format ?? "";
        _original = ItemFormatFields.Parse(format);
        Title = "개별 표시 설정";
        Width = 540; Height = 590; MinWidth = 390; MinHeight = 360;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var layout = new DockPanel { Margin = new Thickness(12) };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var ok = new Button { Content = "확인", IsDefault = true, MinWidth = 68 };
        ok.Click += Save;
        buttons.Children.Add(ok);
        buttons.Children.Add(new Button { Content = "취소", IsCancel = true, MinWidth = 68, Margin = new Thickness(4,0,0,0) });
        DockPanel.SetDock(buttons, Dock.Bottom); layout.Children.Add(buttons);
        DockPanel.SetDock(_error, Dock.Bottom); layout.Children.Add(_error);
        var tabs = new TabControl(); layout.Children.Add(tabs); Content = layout;
        var common = AddTab(tabs, "표시");
        Choice(common, 25, "표시 언어", ["주 언어", "보조 언어", "두 언어"]);
        Choice(common, 21, "제목", ["숨기기", "모든 화면", "첫 화면"]);
        Choice(common, 23, "제목 정렬", ["주 언어 따르기", "보조 언어 따르기", "왼쪽", "가운데", "오른쪽"]);
        Flags(common, 22, [(2,"그림자"), (4,"코드 표시"), (16,"두 언어 줄 교차"), (32,"외곽선"), (64,"정보 패널 숨기기")]);
        Field(common, 71, "코드 조옮김 (0~11)", 0, 11);
        for (int region = 0; region < 2; region++)
        {
            var pane = AddTab(tabs, region == 0 ? "주 언어" : "보조 언어");
            Field(pane, 43 + region, "글꼴");
            Field(pane, 47 + region, "글자 크기 (6~100 pt)", 6, 100);
            Field(pane, 29 + region, "글자색 (#AARRGGBB)", color: true);
            Choice(pane, 31 + region, "가로 정렬", ["왼쪽", "가운데", "오른쪽"], first: 1);
            Field(pane, 45 + region, "세로 위치 (0~100%)", 0, 100);
            int offset = region * 3;
            Flags(pane, 41, [(1 << offset,"굵게"), (2 << offset,"기울임"), (4 << offset,"밑줄")]);
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

    private void Field(Panel pane, int key, string caption, int? min = null, int? max = null, bool color = false)
    {
        string initial = _original.GetValueOrDefault(key, "");
        if (color && int.TryParse(initial, out int argb)) initial = $"#{unchecked((uint)argb):X8}";
        var input = new TextBox { Text = initial };
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
                if (!int.TryParse(value, out int n) || n < min || n > max) throw new FormatException(caption + " 값을 확인하세요.");
            }
            if (value.Contains('>')) throw new FormatException(caption + "에 > 문자를 사용할 수 없습니다.");
            fields[key] = value;
        });
    }

    private void Choice(Panel pane, int key, string caption, string[] choices, int first = 0)
    {
        int index = int.TryParse(_original.GetValueOrDefault(key), out int n) && n >= first && n < first + choices.Length ? n - first + 1 : 0;
        var input = new ComboBox { ItemsSource = choices.Prepend("기본 설정 따르기"), SelectedIndex = index };
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
        var row = new WrapPanel { Margin = new Thickness(0,8,0,4) }; pane.Children.Add(row);
        foreach (var (mask, label) in flags)
        {
            bool initial = (original & mask) != 0;
            var toggle = new CheckBox { Content = label, IsChecked = initial, Margin = new Thickness(0,0,8,0) }; row.Children.Add(toggle);
            _save.Add(fields =>
            {
                if (toggle.IsChecked == initial) return;
                if (!int.TryParse(fields.GetValueOrDefault(key), out int current)) current = original;
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
                throw new FormatException("보조 언어의 세로 위치는 주 언어보다 아래에 있어야 합니다.");
            if (!fields.SequenceEqual(_original)) FormatData = ItemFormatFields.Encode(fields);
            DialogResult = true;
        }
        catch (FormatException ex) { _error.Text = ex.Message; }
    }
}
