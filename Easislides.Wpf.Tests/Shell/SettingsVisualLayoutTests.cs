using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class SettingsVisualLayoutTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = FindRepositoryRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    private static string LoadText(string relativePath)
    {
        var repoRoot = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(repoRoot, relativePath), Encoding.UTF8);
    }

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? "";

    private static XElement StyleByKey(XDocument xaml, string key)
        => xaml.Descendants()
            .Single(e => e.Name.LocalName == "Style" && Attr(e, "Key") == key);

    private static string SetterValue(XElement style, string property)
        => style.Elements()
            .Where(e => e.Name.LocalName == "Setter")
            .Where(e => Attr(e, "Property") == property)
            .Select(e => Attr(e, "Value"))
            .FirstOrDefault() ?? "";

    [Fact]
    public void GlobalLyricsAlignmentButtons_UseEqualThreeColumnLayout()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var group = window.Descendants()
            .Single(e => e.Name.LocalName == "UniformGrid" && Attr(e, "Name") == "GlobalLyricsAlignmentButtons");

        Attr(group, "Columns").Should().Be("3");
        Attr(group, "Rows").Should().Be("1");
        group.Elements().Where(e => e.Name.LocalName == "Button").Select(e => Attr(e, "CommandParameter"))
            .Should().Equal(
                "{x:Static settings:LyricsTextAlignment.Left}",
                "{x:Static settings:LyricsTextAlignment.Center}",
                "{x:Static settings:LyricsTextAlignment.Right}");
    }

    [Fact]
    public void SelectedItemAlignmentButtons_KeepLeftCenterRightInEqualThreeColumnLayout()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var group = window.Descendants()
            .Single(e => e.Name.LocalName == "UniformGrid" && Attr(e, "Name") == "SelectedItemAlignmentButtons");

        Attr(group, "Columns").Should().Be("3");
        Attr(group, "Rows").Should().Be("1");
        group.Elements().Where(e => e.Name.LocalName == "Button").Select(e => Attr(e, "CommandParameter"))
            .Should().Equal("1", "2", "3");
    }

    [Fact]
    public void SettingsTextInputs_UseClippingSafeStyle()
    {
        var xaml = LoadText("Easislides.Wpf/MainWindow.xaml");

        xaml.Should().Contain("x:Key=\"ClassicSettingsTextBox\"");
        xaml.Should().Contain("Style=\"{StaticResource ClassicSettingsTextBox}\"");
        xaml.Should().Contain("Text=\"{Binding LyricsFontSizeInput");
        xaml.Should().Contain("Text=\"{Binding LyricsLineSpacingInput");
        xaml.Should().Contain("Text=\"{Binding LyricsLeftMarginInput");
        xaml.Should().Contain("Text=\"{Binding NewAppearanceTemplateName");
    }

    [Fact]
    public void SettingsTextInputStyles_ReserveEnoughHeightAndPadding()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var normal = StyleByKey(window, "ClassicSettingsTextBox");
        SetterValue(normal, "MinHeight").Should().Be("36");
        SetterValue(normal, "Padding").Should().Be("8,6");
        SetterValue(normal, "VerticalContentAlignment").Should().Be("Center");

        var compact = StyleByKey(window, "ClassicCompactSettingsTextBox");
        SetterValue(compact, "Height").Should().Be("34");
        SetterValue(compact, "MinHeight").Should().Be("34");
        SetterValue(compact, "Padding").Should().Be("7,4");
    }

    [Fact]
    public void IndividualSettingsCheckbox_UsesClippingSafeStyle()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var style = StyleByKey(window, "ClassicSettingsCheckBox");
        SetterValue(style, "MinHeight").Should().Be("30");
        SetterValue(style, "Padding").Should().Be("2,4");
        SetterValue(style, "VerticalContentAlignment").Should().Be("Center");

        var checkbox = window.Descendants()
            .Single(e => e.Name.LocalName == "CheckBox" && Attr(e, "Name") == "Ind_checkBox");
        Attr(checkbox, "Style").Should().Be("{StaticResource ClassicSettingsCheckBox}");
    }

    [Fact]
    public void SettingsComboBoxes_ReserveEnoughHeightAndPadding()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var comboStyle = StyleByKey(window, "ClassicSettingsComboBox");
        Attr(comboStyle, "BasedOn").Should().Be("{StaticResource EsComboBox.Default}");
        SetterValue(comboStyle, "MinHeight").Should().Be("34");
        SetterValue(comboStyle, "Padding").Should().Be("8,4");
        SetterValue(comboStyle, "VerticalContentAlignment").Should().Be("Center");

        var transitionCombos = new[]
        {
            "Def_TransItem",
            "Def_TransSlides",
            "Ind_TransItem",
            "Ind_TransSlides",
        };

        foreach (var name in transitionCombos)
        {
            var combo = window.Descendants()
                .Single(e => e.Name.LocalName == "ComboBox" && Attr(e, "Name") == name);

            Attr(combo, "Style").Should().Be("{StaticResource ClassicSettingsComboBox}", $"{name} should use the clipping-safe settings combo style");
            Attr(combo, "Height").Should().Be("", $"{name} should let the settings combo style own height");
        }
    }

    [Fact]
    public void SelectedItemImageAndMediaPathTextBoxes_UseCompactClippingSafeStyle()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var pathBindings = new[]
        {
            "SelectedItemBackgroundImagePath, Mode=OneWay",
            "SelectedItemMediaPath, Mode=OneWay",
        };

        foreach (var binding in pathBindings)
        {
            var textBox = window.Descendants()
                .Single(e => e.Name.LocalName == "TextBox" && Attr(e, "Text").Contains(binding, StringComparison.Ordinal));

            Attr(textBox, "Style").Should().Be("{StaticResource ClassicCompactSettingsTextBox}", $"{binding} should use the compact settings text style");
            Attr(textBox, "Height").Should().Be("", $"{binding} should not keep a clipping-prone fixed height");
        }
    }

    [Fact]
    public void EsTextBoxContentHost_DoesNotForceCenterClipping()
    {
        var textBoxXaml = LoadText("Easislides.Wpf/Controls/EsTextBox.xaml");

        textBoxXaml.Should().NotContain("x:Name=\"PART_ContentHost\"\r\n                                      Margin=\"{TemplateBinding Padding}\"\r\n                                      VerticalAlignment=\"Center\"");
        textBoxXaml.Should().NotContain("x:Name=\"PART_ContentHost\"\n                                      Margin=\"{TemplateBinding Padding}\"\n                                      VerticalAlignment=\"Center\"");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Easislides.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to find repository root.");
    }
}
