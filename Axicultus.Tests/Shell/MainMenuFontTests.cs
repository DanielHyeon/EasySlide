using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// WPF-UI's app-wide AccessText style draws a header with an access key ("_Import") at 14 px while the
// others read at the menu's size (the Tools menu looked larger). An implicit style inside a template comes only from
// the application's resources, so the main menu's items draw their header with an AccessText that takes no style.
public sealed class MainMenuFontTests
{
    [Fact]
    public void MainMenuHeadersTakeNoAppWideAccessTextStyle()
    {
        var xaml = XDocument.Load(Path.Combine(Root(), "Axicultus", "MainWindow.xaml"));
        var menu = xaml.Descendants().Single(e => e.Name.LocalName == "Menu" && (string?)e.Attribute("AutomationProperties.Name") == "주 메뉴");
        var style = menu.Elements().Single(e => e.Name.LocalName == "Menu.Resources").Elements().Single(e => e.Name.LocalName == "Style");
        Assert.Equal("MenuItem", (string?)style.Attribute("TargetType"));
        Assert.Null(style.Attribute(XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml"))); // implicit: every submenu item
        Assert.Equal("{StaticResource {x:Type MenuItem}}", (string?)style.Attribute("BasedOn")); // WPF-UI's look otherwise
        var setter = style.Elements().Single(e => (string?)e.Attribute("Property") == "HeaderTemplate");
        var header = setter.Descendants().Single(e => e.Name.LocalName == "DataTemplate").Elements().Single();
        Assert.Equal("AccessText", header.Name.LocalName);
        Assert.Equal("{Binding}", (string?)header.Attribute("Text"));
        Assert.Equal("{x:Null}", (string?)header.Attribute("Style"));
        Assert.Null(header.Attribute("FontSize")); // the item's size, inherited
    }

    private static string Root()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Axicultus.sln"))) return dir.FullName;
        throw new DirectoryNotFoundException("Axicultus.sln");
    }
}
