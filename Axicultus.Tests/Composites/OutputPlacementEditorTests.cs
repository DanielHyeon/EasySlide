using System.IO;
using System.Linq;
using System.Xml.Linq;
using Axicultus.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Composites;

public class OutputPlacementEditorTests
{
    private static XDocument Read(string path) => XDocument.Load(Path.Combine(XamlAccessibilityScanner.FindRepoRoot(), "Axicultus", path));

    [Fact]
    public void MonitorTabHostsCustomBoundsWithTheWinFormsTwoRowOrder()
    {
        var window = Read("SettingsWindow.xaml");
        var host = window.Descendants().Single(e => e.Name.LocalName == "OutputPlacementEditor");
        host.Ancestors().First(e => e.Name.LocalName == "TabItem").Attribute("Header")!.Value.Should().Be("{loc:Text SettingsWindow.TabMonitors}");

        var editors = Read("Composites/OutputPlacementEditor.xaml").Descendants()
            .Where(e => e.Name.LocalName is "OptionNumberBox" or "TextBox")
            .OrderBy(e => (int)e.Attribute("Canvas.Top")!)
            .ThenBy(e => (int)e.Attribute("Canvas.Left")!).ToArray();
        editors.Select(e => e.Attribute("Text")!.Value.Split(' ', ',')[1]).Should()
            .Equal("DisplayCustomTop", "DisplayCustomWidth", "DisplayCustomLeft", "AudienceHeight");
        editors.Take(3).Should().OnlyContain(e => (string?)e.Attribute("IsEnabled") == "{Binding UseCustomOutputBounds}");
        editors[3].Attribute("IsReadOnly")!.Value.Should().Be("True");
    }

    [Fact]
    public void MainWindowOptionsOfferAllInterfaceSizesInline()
    {
        var window = Read("SettingsWindow.xaml");
        var choices = window.Descendants().Where(e => (string?)e.Attribute("GroupName") == "InterfaceSize").ToArray();
        choices.Select(e => (string?)e.Attribute("Content")).Should().Equal(
            "{loc:Text SettingsWindow.SizeSmall}", "{loc:Text SettingsWindow.SizeStandard}", "{loc:Text SettingsWindow.SizeLarge}", "{loc:Text SettingsWindow.SizeSenior}");
        foreach (var (choice, size) in choices.Zip(new[] { "Small", "Standard", "Large", "Senior" }))
        {
            choice.Ancestors().First(e => e.Name.LocalName == "TabItem").Attribute("Header")!.Value.Should().Be("{loc:Text SettingsWindow.TabMainWindow}");
            choice.Attribute("IsChecked")!.Value.Should().Contain("Binding InterfaceSize, Mode=TwoWay").And.Contain("ConverterParameter=" + size);
        }
    }

    [Fact]
    public void OperatorOptionsAndWpfSettingsHaveSeparateEditors()
    {
        var window = Read("SettingsWindow.xaml");
        var tabs = window.Descendants().First(e => e.Name.LocalName == "TabControl").Elements().ToArray();
        var advanced = tabs.Single(tab => (string?)tab.Attribute("Header") == "{loc:Text SettingsWindow.TabMore}");
        var keyboard = tabs.Single(tab => (string?)tab.Attribute("Header") == "{loc:Text SettingsWindow.TabKeyboard}");
        var shortcuts = window.Descendants().Single(e => (string?)e.Attribute("ItemsSource") == "{Binding ShortcutItems}");
        shortcuts.Ancestors().Should().Contain(keyboard);

        var classicControls = tabs.TakeWhile(tab => tab != advanced).SelectMany(tab => tab.Descendants())
            .Concat(Read("Composites/OutputPlacementEditor.xaml").Descendants());
        static string[] EditablePaths(System.Collections.Generic.IEnumerable<XElement> controls) => controls
            .Where(e => (string?)e.Attribute("IsReadOnly") != "True").Attributes()
            .Where(a => a.Name.LocalName is "Text" or "Value" or "SelectedItem" or "SelectedIndex" or "SelectedValue" or "IsChecked" or "Argb")
            .Where(a => a.Value.StartsWith("{Binding ", System.StringComparison.Ordinal))
            .Select(a => a.Value[9..].Split(',', '}')[0]).Distinct().ToArray();
        EditablePaths(advanced.Descendants()).Intersect(EditablePaths(classicControls)).Should().BeEmpty();
    }

    [Fact]
    public void NativeOptionsPreserveTabOrderAndAlertToolbar()
    {
        var window = Read("SettingsWindow.xaml");
        var pages = window.Descendants().First(e => e.Name.LocalName == "TabControl").Elements()
            .Where(e => e.Name.LocalName == "TabItem").ToArray();
        pages.Take(8).Select(e => (string?)e.Attribute("Header")).Should().Equal(
            new[] { "TabMainWindow", "TabShow", "TabMonitors", "TabAlerts", "TabFolders", "TabBibles", "TabLicence", "TabKeyboard" }
                .Select(key => "{loc:Text SettingsWindow." + key + "}"));
        foreach (var alert in new[] { "{loc:Text SettingsWindow.MessageAlert}", "{loc:Text SettingsWindow.ParentalAlert}", "{loc:Text SettingsWindow.DisplayReference}" })
        {
            var group = pages[3].Descendants().Single(e => (string?)e.Attribute("Header") == alert);
            group.Descendants().Where(e => e.Name.LocalName == "FrmIcon").Select(e => (string?)e.Attribute("Icon"))
                .Should().ContainInOrder("text_direction_horizontal_left", "flash", "transparency_square", "text_bold",
                    "text_italic", "text_underline", "text_shadow", "text_effects");
        }
    }
}
