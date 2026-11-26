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
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Controls;

public sealed class OperatorOptionPanel : StackPanel
{
    // The label is the text named after the option.
    private sealed record Field(string Name, string[]? Choices = null)
    {
        public string Label => UiText.Get(nameof(OperatorOptionPanel), Name);
    }
    private static readonly string[] Vertical = [UiText.Get(nameof(OperatorOptionPanel), "ChoiceTop"), UiText.Get(nameof(OperatorOptionPanel), "ChoiceMiddle"), UiText.Get(nameof(OperatorOptionPanel), "ChoiceBottom")];
    private static readonly string[] Horizontal = [UiText.Get(nameof(OperatorOptionPanel), "ChoiceLeft"), UiText.Get(nameof(OperatorOptionPanel), "ChoiceCenter"), UiText.Get(nameof(OperatorOptionPanel), "ChoiceRight")];
    private static readonly Dictionary<string, Field[]> Groups = new()
    {
        ["Preview"] = [
            new("PreviewFontPoints"),
            new("PreviewDivider"),
            new("PreviewFocusColorEnabled"),
            new("PreviewFocusArgb"),
            new("PreviewSelectionTextArgb"),
            new("PreviewSelectionBackgroundArgb"),
            new("EditHistoryLimit"),
        ],
        ["Video"] = [
            new("BackgroundVideoPercent"),
            new("BackgroundVideoAlignment", Vertical),
            new("LargestFont"),
            new("RegionDivider"),
            new("HangingIndent"),
            new("NotationPercent"),
            new("PreventScreenSaver"),
        ],
        ["StageVisual"] = [
            new("StageFontPoints"),
            new("StageNotationPoints"),
            new("StageShowNotations"),
            new("StageTextArgb"),
            new("StageBackgroundArgb"),
            new("StageBold"),
            new("StageItalic"),
            new("StageUnderline"),
        ],
        ["Audience"] = [new("OutputAsSingle")],
        ["Stage"] = [
            new("StagePreferSecondary"),
            new("StageCustom"),
            new("StageLeft"),
            new("StageTop"),
            new("StageWidth"),
        ],
        ["Camera"] = [
            new("CameraWide"),
            new("CameraHidePanel"),
            new("CameraVolume"),
            new("CameraBalance"),
            new("CameraMuted"),
        ],
        ["Keyboard"] = [
            new("AlternateKeyboard"),
            new("GlobalF7"),
            new("GlobalF8"),
            new("GlobalF9"),
            new("GlobalF10"),
            new("GlobalArrow"),
            new("GlobalCtrlArrow"),
        ],
        ["Message"] = [
            new("MessageFamily"),
            new("MessagePoints"),
            new("MessageForegroundArgb"),
            new("MessageBackgroundArgb"),
            new("MessageHorizontal", Horizontal),
            new("MessageVertical", Vertical),
            new("MessageBold"),
            new("MessageItalic"),
            new("MessageUnderline"),
            new("MessageShadow"),
            new("MessageOutline"),
        ],
        ["Parental"] = [
            new("ParentalFamily"),
            new("ParentalPoints"),
            new("ParentalForegroundArgb"),
            new("ParentalBackgroundArgb"),
            new("ParentalHorizontal", Horizontal),
            new("ParentalVertical", Vertical),
            new("ParentalBold"),
            new("ParentalItalic"),
            new("ParentalUnderline"),
            new("ParentalShadow"),
            new("ParentalOutline"),
        ],
        ["Reference"] = [
            new("ReferenceFamily"),
            new("ReferencePoints"),
            new("ReferenceForegroundArgb"),
            new("ReferenceBackgroundArgb"),
            new("ReferenceHorizontal", Horizontal),
            new("ReferenceVertical", Vertical),
            new("ReferenceBold"),
            new("ReferenceItalic"),
            new("ReferenceUnderline"),
            new("ReferenceShadow"),
            new("ReferenceOutline"),
        ],
    };

    private static readonly Lazy<string[]> InstalledFonts = new(() => [.. Fonts.SystemFontFamilies.Select(f => f.Source).Order()]);

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
