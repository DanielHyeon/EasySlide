using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Controls;

public sealed class OperatorOptionPanel : StackPanel
{
    private sealed record Field(string Name, string Label, string[]? Choices = null);
    private static readonly Dictionary<string, Field[]> Groups = new()
    {
        ["Preview"] = [
            new("PreviewFontPoints", "미리보기 글자 크기 (pt)"),
            new("PreviewDivider", "화면 사이 구분선"),
            new("PreviewFocusColorEnabled", "포커스 영역 배경색 사용"),
            new("PreviewFocusArgb", "포커스 배경색"),
            new("PreviewSelectionTextArgb", "선택한 슬라이드 글자색"),
            new("PreviewSelectionBackgroundArgb", "선택한 슬라이드 배경색"),
            new("EditHistoryLimit", "최근 편집 항목 수"),
        ],
        ["Video"] = [
            new("BackgroundVideoPercent", "배경 동영상 크기 (%)"),
            new("BackgroundVideoAlignment", "배경 동영상 위치", ["위", "가운데", "아래"]),
            new("LargestFont", "영역에 맞는 최대 글꼴 사용"),
            new("RegionDivider", "두 가사 영역 사이 구분선"),
            new("HangingIndent", "왼쪽 정렬 시 이어지는 줄 들여쓰기"),
            new("NotationPercent", "코드 글꼴 크기 (%)"),
            new("PreventScreenSaver", "송출 중 화면 보호기 억제"),
        ],
        ["StageVisual"] = [
            new("StageFontPoints", "가사 글자 크기 (pt)"),
            new("StageNotationPoints", "코드 글자 크기 (pt)"),
            new("StageShowNotations", "가사 모니터 코드 표기"),
            new("StageTextArgb", "가사 글자색"),
            new("StageBackgroundArgb", "가사 배경색"),
            new("StageBold", "굵게"),
            new("StageItalic", "기울임"),
            new("StageUnderline", "밑줄"),
        ],
        ["Audience"] = [new("OutputAsSingle", "사용자 지정 송출을 단일 모니터처럼 사용")],
        ["Stage"] = [
            new("StagePreferSecondary", "보조 가사 모니터 우선"),
            new("StageCustom", "가사 모니터 사용자 위치 사용"),
            new("StageLeft", "가사 모니터 왼쪽 (px)"),
            new("StageTop", "가사 모니터 위쪽 (px)"),
            new("StageWidth", "가사 모니터 너비 (px)"),
        ],
        ["Camera"] = [
            new("CameraWide", "카메라 와이드 화면"),
            new("CameraHidePanel", "카메라 송출 패널 숨김"),
            new("CameraVolume", "카메라 음량"),
            new("CameraBalance", "카메라 좌우 밸런스"),
            new("CameraMuted", "카메라 음소거"),
        ],
        ["Keyboard"] = [
            new("AlternateKeyboard", "두 번째 키보드 배열 사용"),
            new("GlobalF7", "전역 F7"),
            new("GlobalF8", "전역 F8"),
            new("GlobalF9", "전역 F9"),
            new("GlobalF10", "전역 F10"),
            new("GlobalArrow", "전역 방향키"),
            new("GlobalCtrlArrow", "전역 Ctrl + 방향키"),
        ],
        ["Message"] = [
            new("MessageFamily", "글꼴"),
            new("MessagePoints", "글자 크기 (pt)"),
            new("MessageForegroundArgb", "글자색"),
            new("MessageBackgroundArgb", "배경색"),
            new("MessageHorizontal", "가로 정렬", ["왼쪽", "가운데", "오른쪽"]),
            new("MessageVertical", "세로 정렬", ["위", "가운데", "아래"]),
            new("MessageBold", "굵게"),
            new("MessageItalic", "기울임"),
            new("MessageUnderline", "밑줄"),
            new("MessageShadow", "그림자"),
            new("MessageOutline", "외곽선"),
        ],
        ["Parental"] = [
            new("ParentalFamily", "글꼴"),
            new("ParentalPoints", "글자 크기 (pt)"),
            new("ParentalForegroundArgb", "글자색"),
            new("ParentalBackgroundArgb", "배경색"),
            new("ParentalHorizontal", "가로 정렬", ["왼쪽", "가운데", "오른쪽"]),
            new("ParentalVertical", "세로 정렬", ["위", "가운데", "아래"]),
            new("ParentalBold", "굵게"),
            new("ParentalItalic", "기울임"),
            new("ParentalUnderline", "밑줄"),
            new("ParentalShadow", "그림자"),
            new("ParentalOutline", "외곽선"),
        ],
        ["Reference"] = [
            new("ReferenceFamily", "글꼴"),
            new("ReferencePoints", "글자 크기 (pt)"),
            new("ReferenceForegroundArgb", "글자색"),
            new("ReferenceBackgroundArgb", "배경색"),
            new("ReferenceHorizontal", "가로 정렬", ["왼쪽", "가운데", "오른쪽"]),
            new("ReferenceVertical", "세로 정렬", ["위", "가운데", "아래"]),
            new("ReferenceBold", "굵게"),
            new("ReferenceItalic", "기울임"),
            new("ReferenceUnderline", "밑줄"),
            new("ReferenceShadow", "그림자"),
            new("ReferenceOutline", "외곽선"),
        ],
    };

    private static readonly Lazy<string[]> InstalledFonts = new(() => Fonts.SystemFontFamilies.Select(f => f.Source).Order().ToArray());

    public string Group { get; set; } = "";

    public OperatorOptionPanel()
    {
        Loaded += (_, _) => Populate();
        DataContextChanged += (_, _) => { if (IsLoaded) Populate(); };
    }

    private void Populate()
    {
        Children.Clear();
        if (DataContext is not SettingsWindowViewModel vm || !Groups.TryGetValue(Group, out var fields)) return;
        foreach (var field in fields)
        {
            var option = OperatorOptionCatalog.ById["operator." + field.Name];
            var row = new OptionRow(vm, option);
            var label = field.Label;
            Control editor;
            if (option.Default is bool)
            {
                editor = new CheckBox { Content = label };
                editor.SetBinding(CheckBox.IsCheckedProperty, Bind(nameof(row.Value), row));
            }
            else
            {
                if (field.Choices is { } choices)
                {
                    var combo = new ComboBox { ItemsSource = choices };
                    combo.SetBinding(ComboBox.SelectedIndexProperty, Bind(nameof(row.Index), row));
                    editor = combo;
                }
                else if (field.Name.EndsWith("Argb", StringComparison.Ordinal))
                {
                    var button = new Button();
                    button.SetBinding(ContentControl.ContentProperty, Bind(nameof(row.Hex), row, BindingMode.OneWay));
                    button.Click += (_, _) =>
                    {
                        using var dialog = new System.Windows.Forms.ColorDialog { Color = System.Drawing.Color.FromArgb((int)row.Value), FullOpen = true };
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) row.Value = dialog.Color.ToArgb();
                    };
                    editor = button;
                }
                else if (option.Default is string)
                {
                    var fonts = new ComboBox { ItemsSource = InstalledFonts.Value, IsEditable = true };
                    fonts.SetBinding(ComboBox.TextProperty, Bind(nameof(row.Value), row));
                    editor = fonts;
                }
                else
                {
                    var number = new TextBox();
                    number.SetBinding(TextBox.TextProperty, Bind(nameof(row.Number), row));
                    editor = number;
                    if (option.Minimum is int min) label += $" ({min}–{option.Maximum})";
                }
                Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 4, 0, 1) });
            }
            AutomationProperties.SetName(editor, field.Label);
            Children.Add(editor);
        }
    }

    private static Binding Bind(string path, OptionRow source, BindingMode mode = BindingMode.TwoWay) =>
        new(path) { Source = source, Mode = mode, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, ValidatesOnExceptions = true };

    private sealed class OptionRow : ObservableObject
    {
        private readonly SettingsWindowViewModel vm;
        private readonly OperatorOption option;
        internal OptionRow(SettingsWindowViewModel vm, OperatorOption option)
        {
            this.vm = vm;
            this.option = option;
            PropertyChangedEventManager.AddHandler(vm, (_, _) => OnPropertyChanged(string.Empty), nameof(vm.OperatorPreferences));
        }
        public object Value
        {
            get => vm.ReadOperatorOption(option);
            set
            {
                vm.WriteOperatorOption(option, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Hex));
            }
        }
        public string Hex => $"#{(uint)(int)Value & 0xffffff:X6}";
        public int Index { get => (int)Value - option.Minimum!.Value; set { if (value >= 0) Value = value + option.Minimum!.Value; } }
        public int Number { get => (int)Value; set => Value = value; }
    }
}
