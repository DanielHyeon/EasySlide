using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using Axicultus.Wpf.Composites;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Tests.Accessibility;
using Axicultus.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a044: the main window's text follows the interface size (Standard 11, Small 10 DIP), and a narrow window neither draws
// a scroll bar over the bottom tabs nor cuts worship-list titles or the status bar mid-character.
[Collection("WPF Application")]
public sealed class MainWindowTextSizeTests
{
    private const string BodyToken = "{DynamicResource Type.Body.FontSize}";
    private static readonly string[] BottomTabs = ["Folders", "InfoScr", "PowerP", "Bibles", "Images", "Default"];

    private static XDocument Load(string path) => XDocument.Load(Path.Combine(XamlAccessibilityScanner.FindRepoRoot(), "Axicultus", path));

    private static string Attr(XElement e, string name) => e.Attributes().FirstOrDefault(a => a.Name.LocalName == name)?.Value ?? "";

    private static void EnsureStyles()
    {
        var resources = Application.Current.Resources;
        foreach (var (key, file) in new[] { ("FrmMainButton", "Controls/EsButton.xaml"), ("EsTabView.FrmMainBottom", "Controls/EsTabView.xaml") })
        {
            if (!resources.Contains(key))
                resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"pack://application:,,,/Axicultus;component/{file}", UriKind.Absolute) });
        }
    }

    private static void Layout(FrameworkElement element, Size size)
    {
        for (int pass = 0; pass < 2; pass++)
        {
            element.Measure(size);
            element.Arrange(new Rect(size));
            element.UpdateLayout();
        }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var deeper in Descendants(child)) yield return deeper;
        }
    }

    private static Rect Bounds(FrameworkElement element, Visual root)
        => element.TransformToAncestor(root).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

    // The text's width with no limit, measured the way the TextBlock measures it.
    private static double TextWidth(TextBlock text)
    {
        var probe = new TextBlock { Text = text.Text, FontFamily = text.FontFamily, FontSize = text.FontSize, FontStyle = text.FontStyle,
            FontWeight = text.FontWeight, FontStretch = text.FontStretch, UseLayoutRounding = text.UseLayoutRounding };
        probe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return probe.DesiredSize.Width;
    }

    [Theory]
    [InlineData(InterfaceSize.Standard, 11.0)]
    [InlineData(InterfaceSize.Small, 10.0)]
    [InlineData(InterfaceSize.Large, 14 * 1.2)]
    [InlineData(InterfaceSize.Senior, 14 * 1.4)]
    public void BodyButtonAndControlDefaultsTakeTheSizesBodyFont(InterfaceSize size, double body)
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            var resources = Application.Current.Resources;
            foreach (var key in new[] { "Type.Body.FontSize", "Type.Button.FontSize", "ContentControlFontSize", "ControlContentThemeFontSize", "DefaultDataGridFontSize", "TreeViewItemFontSize" })
                resources[key].Should().Be(body, key);
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    }

    [Theory]
    [InlineData(InterfaceSize.Standard, 11.0)]
    [InlineData(InterfaceSize.Small, 10.0)]
    [InlineData(InterfaceSize.Large, 12 * 1.2)]
    [InlineData(InterfaceSize.Senior, 12 * 1.4)]
    public void CaptionIsOneStepSmallerAtStandardAndSmallOnly(InterfaceSize size, double caption)
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            Application.Current.Resources["Type.Caption.FontSize"].Should().Be(caption);
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    }

    // The caption console and the settings pages draw with these; they stay as they were.
    [Theory]
    [InlineData(InterfaceSize.Standard, 1.0)]
    [InlineData(InterfaceSize.Small, 11.0 / 12.0)]
    [InlineData(InterfaceSize.Large, 1.2)]
    [InlineData(InterfaceSize.Senior, 1.4)]
    public void TitleAndLargeTextTokensKeepTheirSizes(InterfaceSize size, double factor)
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            var resources = Application.Current.Resources;
            foreach (var (key, standard) in new[] { ("Type.Display.FontSize", 32.0), ("Type.Title1.FontSize", 24.0), ("Type.Title2.FontSize", 20.0),
                         ("Type.Title3.FontSize", 18.0), ("Type.BodyLarge.FontSize", 16.0), ("Type.ButtonLarge.FontSize", 16.0) })
                resources[key].Should().Be(standard * factor, key);
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    }

    [Fact]
    public void TheTypographyDefaultsMatchStandard()
    {
        var tokens = Load("Theme/Tokens/Typography.xaml").Root!.Elements()
            .Where(e => e.Name.LocalName == "Double").ToDictionary(e => Attr(e, "Key"), e => e.Value);
        foreach (var key in new[] { "Type.Body.FontSize", "Type.Caption.FontSize", "Type.Button.FontSize" })
            tokens[key].Should().Be("11", key);
    }

    // Scenario: Standard 11 / switching to Small gives 10 — menus, worship list, song list, bottom tabs, context menus.
    [Theory]
    [InlineData(InterfaceSize.Standard, 11.0)]
    [InlineData(InterfaceSize.Small, 10.0)]
    public void WorshipListBottomTabsAndContextMenusFollowTheBodySize(InterfaceSize size, double expected) => StaHelper.RunOnSta(() =>
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            EnsureStyles();
            var resources = Application.Current.Resources;
            var tab = new TabItem { Style = (Style)resources["EsTabView.FrmMainBottomItem"], Header = "Folders" };
            var menu = new ContextMenu { Style = (Style)resources["FrmMainListContextMenu"] };
            var panel = new WorshipListPanel();
            tab.FontSize.Should().Be(expected, "the bottom tabs");
            menu.FontSize.Should().Be(expected, "the list context menus");
            panel.QueueList.FontSize.Should().Be(expected, "the worship list");
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    });

    [Fact]
    public void MainWindowTextTakesTheBodyTokenInsteadOfAFixedSize()
    {
        var window = Load("MainWindow.xaml");
        var menu = window.Descendants().Single(e => e.Name.LocalName == "Menu" && Attr(e, "AutomationProperties.Name") == "주 메뉴");
        Attr(menu, "FontSize").Should().Be(BodyToken, "the menu bar");
        foreach (var name in new[] { "LibrarySongList", "PraiseBookItems", "BookLookup", "BibleUserLookup" })
            Attr(window.Descendants().Single(e => Attr(e, "Name") == name), "FontSize").Should().Be(BodyToken, name);

        foreach (var file in new[] { "MainWindow.xaml", "Composites/WorshipListPanel.xaml" })
        {
            static bool Literal(string value) => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
            var fixedSizes = Load(file).Descendants()
                .Where(e => e.Attributes().Any(a => a.Name.LocalName.EndsWith("FontSize", StringComparison.Ordinal) && Literal(a.Value))
                    || e.Name.LocalName == "Setter" && Attr(e, "Property") == "FontSize" && Literal(Attr(e, "Value")))
                // Deliberate exemptions: none of these is text that reads at the interface size.
                .Where(e => e.Name.LocalName != "SymbolIcon") // icon glyphs sized to their buttons
                .Where(e => Attr(e, "Text") != "{loc:Text MainWindow.AppearanceSample}") // the colour sample inside its fixed 34x22 swatch
                .Where(e => !e.Ancestors().Any(a => Attr(a, "Name") == "CommandPaletteOverlay")) // the palette overlay keeps its own scale
                .Where(e => !e.Ancestors().Any(a => Attr(a, "Key") == "DefaultSpinnerButton")) // the spinner's arrow glyphs
                .Where(e => !e.Ancestors().Any(a => Attr(a, "Name") == "ClassicWorshipListToolStrip2")) // the icon-only side rail
                .Select(e => $"{file}: <{e.Name.LocalName} {Attr(e, "Name")}{Attr(e, "Key")}{Attr(e, "Text")} {Attr(e, "FontSize")}{Attr(e, "Value")}>");
            fixedSizes.Should().BeEmpty("main-window text follows the interface size");
        }
    }

    // Scenario: at the 780x540 minimum the six source tabs wrap into rows and no scroll bar covers their labels.
    [Fact]
    public void BottomTabsWrapIntoRowsWithoutAScrollBarAtTheMinimumWidth() => StaHelper.RunOnSta(() =>
    {
        new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        EnsureStyles();
        var tabs = new TabControl { Style = (Style)Application.Current.Resources["EsTabView.FrmMainSourceTabs"], TabStripPlacement = Dock.Bottom };
        foreach (var name in BottomTabs) tabs.Items.Add(new TabItem { Header = name, Content = new Border() });
        tabs.SelectedIndex = 3;
        var size = new Size(190, 300); // the source pane when the window is at its minimum size
        Layout(tabs, size);

        var strip = Descendants(tabs).OfType<TabPanel>().Single();
        for (DependencyObject? at = strip; at != null && at != tabs; at = VisualTreeHelper.GetParent(at))
            at.Should().NotBeOfType<ScrollViewer>("an overlay scroll bar is drawn over the tab labels");
        var items = tabs.Items.Cast<TabItem>().ToArray();
        foreach (var item in items)
        {
            var box = Bounds(item, tabs);
            box.Width.Should().BeGreaterThanOrEqualTo(item.DesiredSize.Width - 0.5, $"{item.Header} shows its whole label");
            box.Left.Should().BeGreaterThanOrEqualTo(-0.5, $"{item.Header}");
            box.Right.Should().BeLessThanOrEqualTo(size.Width + 0.5, $"{item.Header} stays inside the pane");
            box.Bottom.Should().BeLessThanOrEqualTo(size.Height + 0.5, $"{item.Header}");
        }
        items.Select(i => Math.Round(Bounds(i, tabs).Top)).Distinct().Count().Should().BeGreaterThan(1, "the tabs wrap into rows");
        Bounds(items[3], tabs).Top.Should().BeApproximately(Bounds(strip, tabs).Top, 0.5, "the selected tab's row sits against its content");
    });

    // Scenario: a long worship-list title fills the list's width and ends with an ellipsis inside it.
    [Fact]
    public void ALongWorshipTitleEndsWithAnEllipsisInsideTheList() => StaHelper.RunOnSta(() =>
    {
        new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        EnsureStyles();
        const string title = "621장_찬양하라_내_영혼아_새찬송가악보_PPTX.pptx";
        var panel = new WorshipListPanel { DataContext = new QueueHost(new LiveQueueItem("ppt:621", title, LiveItemKinds.PowerPoint)) };
        Layout(panel, new Size(220, 400));

        var list = panel.QueueList;
        var text = Descendants(list).OfType<TextBlock>().Single(t => t.Text == title);
        var listBox = Bounds(list, panel);
        var textBox = Bounds(text, panel);
        text.TextTrimming.Should().Be(TextTrimming.CharacterEllipsis);
        textBox.Right.Should().BeLessThanOrEqualTo(listBox.Right - list.BorderThickness.Right + 0.5, "the title stays inside the list");
        TextWidth(text).Should().BeGreaterThan(text.ActualWidth + 1, "the title is trimmed, so it ends with an ellipsis");
        text.ActualWidth.Should().BeGreaterThan(listBox.Width - 70, "the title fills the row beside the item icon");
        var row = text.Ancestors<FrameworkElement>().First(e => AutomationProperties.GetName(e) == title);
        row.ToolTip.Should().NotBeNull("the whole title stays in the row's tool tip");
    });

    // 1.2: the status bar's texts keep a gap; the monitor name shows whole when there is room and otherwise ends with an
    // ellipsis inside the bar, and the status fills what is left (also when no output monitor is chosen).
    [Theory]
    [InlineData(InterfaceSize.Standard, 1166.0)] // the bar in the default 1180 window
    [InlineData(InterfaceSize.Standard, 766.0)]  // ... in the 780 minimum
    [InlineData(InterfaceSize.Large, 1166.0)]
    [InlineData(InterfaceSize.Large, 766.0)]
    public void StatusBarTextsKeepGapsAndTheMonitorNameTrimsOnlyWhenTheBarIsFull(InterfaceSize size, double width) => StaHelper.RunOnSta(() =>
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            EnsureStyles();
            const string status = "예배 순서 불러옴: 01.주일예배 (21개)";
            const string monitor = @"출력 모니터 \\.\DISPLAY1 (2560×1440)";
            foreach (var outputOn in new[] { true, false })
            {
                var element = StatusBar(status, outputOn ? monitor : null);
                Layout(element, new Size(width, 40));
                var texts = Descendants(element).OfType<TextBlock>().ToArray();
                var statusText = texts.Single(t => t.Text == status);
                var monitorText = texts.Single(t => AutomationProperties.GetName(t) == "출력 모니터");
                var live = Descendants(element).OfType<Border>().Single(b => AutomationProperties.GetName(b) == "Live icon status");
                var room = (FrameworkElement)VisualTreeHelper.GetParent(monitorText); // the part of the bar after the live box
                var statusBox = Bounds(statusText, element);
                var monitorBox = Bounds(monitorText, element);
                var inner = width - ((Border)element).Padding.Right;
                var what = $"{size} at {width}, output {(outputOn ? "on" : "off")}";

                monitorText.TextTrimming.Should().Be(TextTrimming.CharacterEllipsis, what);
                monitorBox.Right.Should().BeLessThanOrEqualTo(inner + 0.5, $"{what}: the monitor name stays in the bar");
                statusBox.Right.Should().BeLessThanOrEqualTo(inner + 0.5, $"{what}: the status stays in the bar");
                (statusBox.Left - Bounds(live, element).Right).Should().BeGreaterThanOrEqualTo(8, $"{what}: a gap after the live box");
                if (!outputOn)
                {
                    monitorText.ActualWidth.Should().Be(0, what);
                    TextWidth(statusText).Should().BeLessThanOrEqualTo(statusText.ActualWidth + 1, $"{what}: the status takes the monitor's room");
                    continue;
                }
                if (statusText.ActualWidth > 0)
                    (monitorBox.Left - statusBox.Right).Should().BeGreaterThanOrEqualTo(8, $"{what}: a gap between the status and the monitor name");
                bool trimmed = TextWidth(monitorText) > monitorText.ActualWidth + 1;
                if (trimmed)
                    monitorText.ActualWidth.Should().BeGreaterThanOrEqualTo(Math.Min(monitorText.MaxWidth, room.ActualWidth) - 1,
                        $"{what}: the monitor name trims only when it has run out of room");
                if (width > 1000)
                {
                    trimmed.Should().BeFalse($"{what}: a default-width bar shows the whole monitor name");
                    TextWidth(statusText).Should().BeLessThanOrEqualTo(statusText.ActualWidth + 1, $"{what}: and the whole status");
                }
                else if (size == InterfaceSize.Standard)
                {
                    statusText.ActualWidth.Should().BeGreaterThan(80, $"{what}: the status is not squeezed out");
                }
            }
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    });

    private static FrameworkElement StatusBar(string status, string? monitor)
    {
        var bar = Load("MainWindow.xaml").Descendants().Single(e => Attr(e, "Name") == "StatusBar");
        bar.Attributes().Where(a => a.Name.LocalName == "Visibility").Remove(); // bound to a menu item outside this element
        bar.SetAttributeValue(XNamespace.Xmlns + "loc", "clr-namespace:Axicultus.Wpf.Localization"); // used only inside attribute values
        var element = (FrameworkElement)XamlReader.Parse(WithAssembly(bar.ToString()));
        element.DataContext = new
        {
            Library = new { BrowserCountText = "Folder: 644 items / " },
            Queue = Enumerable.Range(0, 21).ToList(),
            LyricsPageLabel = "1/2절",
            LiveBar = new { StateLabel = "Off", CurrentItemTitle = "우리교회말씀" },
            StatusText = status,
            SelectedOutputDisplay = monitor == null ? null : new { Name = monitor },
        };
        return element;
    }

    private static string WithAssembly(string xaml) => Regex.Replace(xaml, "clr-namespace:([A-Za-z.]+)\"", "clr-namespace:$1;assembly=Axicultus\"");

    // Tabs wrap only when they must: the six source tabs share one row in the default (290) and 1024-wide (250) panes.
    [Theory]
    [InlineData(290.0)]
    [InlineData(250.0)]
    public void BottomTabsShareOneRowWhenThePaneHasRoom(double width) => StaHelper.RunOnSta(() =>
    {
        new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        EnsureStyles();
        var tabs = new TabControl { Style = (Style)Application.Current.Resources["EsTabView.FrmMainSourceTabs"], TabStripPlacement = Dock.Bottom };
        foreach (var name in BottomTabs) tabs.Items.Add(new TabItem { Header = name, Content = new Border() });
        tabs.SelectedIndex = 0;
        Layout(tabs, new Size(width, 300));
        var items = tabs.Items.Cast<TabItem>().ToArray();
        items.Select(i => Math.Round(Bounds(i, tabs).Top)).Distinct().Should().ContainSingle($"all six tabs fit in {width}");
        foreach (var item in items)
        {
            Bounds(item, tabs).Right.Should().BeLessThanOrEqualTo(width + 0.5, $"{item.Header}");
            item.ActualWidth.Should().BeGreaterThanOrEqualTo(30, $"{item.Header} stays easy to click");
            var label = Descendants(item).OfType<TextBlock>().Single();
            TextWidth(label).Should().BeLessThanOrEqualTo(label.ActualWidth + 0.5, $"{item.Header} shows its whole label");
        }
    });

    // The trailing ellipsis and the LIVE badge end before the list's scroll bar (WPF-UI draws it over the rows), also after
    // the interface size changes while the list is shown and the window is then narrowed.
    [Theory]
    [InlineData(InterfaceSize.Standard)]
    [InlineData(InterfaceSize.Large)]
    [InlineData(InterfaceSize.Senior)]
    public void WorshipRowsEndBeforeTheScrollBar(InterfaceSize size) => StaHelper.RunOnSta(() =>
    {
        try
        {
        new ThemeService().ApplyInterfaceSize(size);
        EnsureStyles();
        var rows = Enumerable.Range(1, 30).Select(n => new LiveQueueItem($"ppt:{n}", $"{n}장_찬양하라_내_영혼아_새찬송가악보_PPTX.pptx", LiveItemKinds.PowerPoint)).ToArray();
        var host = new QueueHost(rows) { LiveRow = rows[1] };
        var panel = new WorshipListPanel { DataContext = host };
        panel.Resources.MergedDictionaries.Add(new global::Wpf.Ui.Markup.ThemesDictionary { Theme = global::Wpf.Ui.Appearance.ApplicationTheme.Light });
        panel.Resources.MergedDictionaries.Add(new global::Wpf.Ui.Markup.ControlsDictionary());
        var body = (double)Application.Current.Resources["Type.Body.FontSize"];
        panel.Resources["Type.Body.FontSize"] = 11.0; // shown at Standard first (a resource on the panel: the app's changes reach windows only)
        Layout(panel, new Size(290, 400));
        panel.Resources["Type.Body.FontSize"] = body;
        Layout(panel, new Size(290, 400));
        Layout(panel, new Size(220, 400));

        var list = panel.QueueList;
        var bar = Descendants(list).OfType<ScrollBar>().Single(b => b.Orientation == Orientation.Vertical && Shown(b));
        var barLeft = Bounds(bar, panel).Left;
        var viewport = Descendants(list).OfType<ScrollViewer>().First().ViewportWidth;
        foreach (var row in Descendants(list).OfType<ListViewItem>().Where(Shown))
            row.ActualWidth.Should().BeApproximately(viewport, 1, "each row spans the list (the LIVE badge stays at its right edge)");
        var titles = Descendants(list).OfType<TextBlock>().Where(t => t.Text.Contains("찬양하라") && Shown(t)).ToArray();
        titles.Should().HaveCountGreaterThan(5);
        foreach (var title in titles)
            Bounds(title, panel).Right.Should().BeLessThanOrEqualTo(barLeft + 0.5, $"{title.Text} ends before the scroll bar");
        var badge = Descendants(list).OfType<Border>().Single(b => Shown(b) && b.Child is TextBlock { Text: var badgeText } && badgeText == Axicultus.Wpf.Localization.UiText.Get("WorshipListPanel", "Live"));
        Bounds(badge, panel).Right.Should().BeLessThanOrEqualTo(barLeft + 0.5, "the LIVE badge ends before the scroll bar");
        ((TextBlock)badge.Child).FontSize.Should().Be((double)Application.Current.Resources["Type.Badge.FontSize"], "the badge scales with the interface size");
        var liveRow = badge.Ancestors<ListViewItem>().First();
        Bounds(badge, panel).Top.Should().BeGreaterThanOrEqualTo(Bounds(liveRow, panel).Top - 0.5, $"{size}: the badge fits its row");
        Bounds(badge, panel).Bottom.Should().BeLessThanOrEqualTo(Bounds(liveRow, panel).Bottom + 0.5, $"{size}: the badge fits its row");
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    });

    [Theory]
    [InlineData(InterfaceSize.Small, 10 * 11.0 / 12.0)]
    [InlineData(InterfaceSize.Standard, 10.0)]
    [InlineData(InterfaceSize.Large, 10 * 1.2)]
    [InlineData(InterfaceSize.Senior, 10 * 1.4)]
    public void TheLiveBadgeScalesWithTheInterfaceSize(InterfaceSize size, double expected)
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            Application.Current.Resources["Type.Badge.FontSize"].Should().Be(expected);
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    }

    // The audience screen keeps the caption size it had before the main window's text went a step smaller.
    [Theory]
    [InlineData(InterfaceSize.Small, 11.0)]
    [InlineData(InterfaceSize.Standard, 12.0)]
    [InlineData(InterfaceSize.Large, 12 * 1.2)]
    [InlineData(InterfaceSize.Senior, 12 * 1.4)]
    public void OutputScreenCaptionsKeepTheirSize(InterfaceSize size, double expected)
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            Application.Current.Resources["Type.Output.Caption.FontSize"].Should().Be(expected);
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
        var output = Load("OutputWindow.xaml");
        foreach (var binding in new[] { "{Binding LyricsAlertText}", "{Binding OutputMonitorName}" })
            Attr(output.Descendants().Single(e => e.Name.LocalName == "TextBlock" && Attr(e, "Text") == binding), "FontSize")
                .Should().Be("{DynamicResource Type.Output.Caption.FontSize}", binding);
        output.Descendants().Where(e => Attr(e, "FontSize").Contains("Type.Body") || Attr(e, "FontSize").Contains("Type.Caption.FontSize")
            || (Attr(e, "Style").Contains("Type.Caption") || Attr(e, "Style").Contains("Type.Body")) && Attr(e, "FontSize") == "")
            .Should().BeEmpty("no audience text takes the main window's body or caption size");
    }

    // Fixed-height boxes grow with a larger interface size instead of cutting their text.
    [Fact]
    public void TextBoxesInFixedStripsDoNotClipAtSenior() => StaHelper.RunOnSta(() =>
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Senior);
            EnsureStyles();
            var window = Load("MainWindow.xaml");
            var resources = WindowResources(window);
            Style Named(string key) => (Style)resources[key];
            XElement Element(string name) => window.Descendants().Single(e => Attr(e, "Name") == name);
            static void Sizes(XElement from, FrameworkElement to)
            {
                if (Attr(from, "Height") is { Length: > 0 } height) to.Height = double.Parse(height, CultureInfo.InvariantCulture);
                if (Attr(from, "MinHeight") is { Length: > 0 } minHeight) to.MinHeight = double.Parse(minHeight, CultureInfo.InvariantCulture);
                if (Attr(from, "FontSize") == BodyToken) to.SetResourceReference(Control.FontSizeProperty, "Type.Body.FontSize");
            }

            var controls = new List<(string Name, FrameworkElement Control)>();
            var colour = new Button { Style = Named("DefaultColourButton"), Content = "Text Colour" };
            controls.Add(("DefaultColourButton", colour));
            foreach (var name in new[] { "Def_TransItem", "Def_TransSlides", "Def_PanelFontList" })
            {
                var combo = new ComboBox { Style = Named("DefaultFieldCombo"), ItemTemplate = (DataTemplate)resources["DefaultStripText"], Width = 104 };
                combo.Items.Add("Fade"); combo.SelectedIndex = 0;
                Sizes(Element(name), combo);
                controls.Add((name, combo));
            }
            var options = new Button { Style = Named("ClassicTopToolStripTextButton"), Content = "새찬송가", DataContext = "새찬송가" }; // hidden without a jump target
            controls.Add(("ClassicTopToolStripTextButton", options));
            var find = new ComboBox { Style = Named("ClassicTopToolStripComboBox"), IsEditable = true, Text = "Search Phrase", Width = 160 };
            controls.Add(("ClassicTopToolStripComboBox", find));
            var book = new ComboBox { Style = null, Width = 120 };
            book.Items.Add("창세기"); book.SelectedIndex = 0;
            Sizes(Element("BookLookup"), book);
            controls.Add(("BookLookup", book));
            var verse = new TextBox { Style = null, Text = "창 1:1", Width = 120, VerticalContentAlignment = VerticalAlignment.Center };
            Sizes(Element("BibleUserLookup"), verse);
            controls.Add(("BibleUserLookup", verse));
            var menuXaml = window.Descendants().Single(e => e.Name.LocalName == "Menu" && Attr(e, "AutomationProperties.Name") == "주 메뉴");
            var menu = new Menu();
            Sizes(menuXaml, menu);
            var file = new MenuItem { Header = "File", Template = (ControlTemplate)resources["FrmMainMenuHeader"] };
            Sizes(menuXaml.Elements().First(e => e.Name.LocalName == "MenuItem"), file);
            menu.Items.Add(file);
            controls.Add(("Menu", menu));

            // Parsed from the window as they are; each sits in a host holding the window's resources, sized to it as in its strip.
            var hosts = new Dictionary<FrameworkElement, FrameworkElement>();
            FrameworkElement Parsed(string name, Func<XElement, bool> which)
            {
                var host = ParseWithWindowResources(which);
                host.HorizontalAlignment = HorizontalAlignment.Left;
                host.VerticalAlignment = VerticalAlignment.Center;
                var element = (FrameworkElement)((Panel)host).Children[0];
                hosts[element] = host;
                controls.Add((name, element));
                return element;
            }
            foreach (var name in new[] { "IndradioButtonText", "IndradioButtonFormat", "IndradioButtonInfo" })
                Parsed(name, e => Attr(e, "Name") == name);
            Parsed("Def_PanelHeight", e => e.Elements().Any(c => Attr(c, "Name") == "Def_PanelHeight")).DataContext = new PanelHeightHost();

            var stack = new StackPanel();
            stack.SetResourceReference(TextElement.FontSizeProperty, "Type.Body.FontSize"); // the main window's own text size, which its controls inherit
            // The Default tab's strips are WrapPanels with ItemHeight=31; the other strips grow, so they get room to.
            foreach (var (name, control) in controls)
                stack.Children.Add(new Border { Height = name.StartsWith("Def", StringComparison.Ordinal) ? 31 : 40, Child = hosts.GetValueOrDefault(control, control) });
            Layout(stack, new Size(400, 2000));
            foreach (var (name, control) in controls)
            {
                var box = Bounds(control, stack);
                if ((control as TextBox ?? Descendants(control).OfType<TextBox>().FirstOrDefault(Shown)) is { } editor)
                {
                    editor.ExtentHeight.Should().BeLessThanOrEqualTo(editor.ViewportHeight + 0.5, $"{name}'s typed text fits its box at Senior");
                    editor.ExtentWidth.Should().BeLessThanOrEqualTo(editor.ViewportWidth + 0.5, $"{name}'s typed text fits its box's width at Senior");
                    Bounds(editor, stack).Bottom.Should().BeLessThanOrEqualTo(box.Bottom + 0.5, name);
                    continue;
                }
                var label = Descendants(control).OfType<FrameworkElement>().FirstOrDefault(e => e is TextBlock { Text.Length: > 0 } or AccessText && Shown(e));
                label.Should().NotBeNull($"{name} shows its text");
                var text = Bounds(label!, stack);
                if (control is ButtonBase button) // a clipped label keeps its measured size, so compare it with the room inside
                {
                    var frame = button.BorderThickness; var padding = button.Padding;
                    (box.Width - frame.Left - frame.Right - padding.Left - padding.Right).Should()
                        .BeGreaterThanOrEqualTo(label!.DesiredSize.Width - 0.5, $"{name} is wide enough for its label at Senior");
                    (box.Height - frame.Top - frame.Bottom - padding.Top - padding.Bottom).Should()
                        .BeGreaterThanOrEqualTo(label.DesiredSize.Height - 0.5, $"{name} is tall enough for its label at Senior");
                }
                text.Left.Should().BeGreaterThanOrEqualTo(box.Left - 0.5, $"{name}'s text starts inside it at Senior ({text} in {box})");
                text.Right.Should().BeLessThanOrEqualTo(box.Right + 0.5, $"{name}'s text ends inside its width at Senior ({text} in {box})");
                label!.DesiredSize.Height.Should().BeGreaterThan(20, $"{name}'s text is Senior-sized");
                box.Height.Should().BeGreaterThanOrEqualTo(label.DesiredSize.Height, $"{name} is as tall as its text at Senior");
                text.Top.Should().BeGreaterThanOrEqualTo(box.Top - 0.5, $"{name}'s text starts inside it at Senior ({text} in {box})");
                text.Bottom.Should().BeLessThanOrEqualTo(box.Bottom + 0.5, $"{name}'s text ends inside it at Senior ({text} in {box})");
            }
            menu.ActualHeight.Should().BeGreaterThan(24, "the menu bar grows with Senior text");
            foreach (var name in new[] { "Def_TransItem", "Def_TransSlides", "Def_PanelFontList", "Def_PanelHeight", "Def_PanelTextColour" })
                Attr(window.Descendants().Single(e => Attr(e, "Name") == name).Ancestors().First(e => e.Name.LocalName == "WrapPanel"), "ItemHeight").Should().Be("31", name);
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    });

    // Shown in its own tree (IsVisible needs a window, these tests lay out without one).
    private static bool Shown(DependencyObject element)
    {
        for (DependencyObject? at = element; at != null; at = VisualTreeHelper.GetParent(at))
            if (at is UIElement { Visibility: not Visibility.Visible }) return false;
        return true;
    }

    // The bottom tabs' chrome is the classic light system colour in both themes; their labels stay dark on it in dark theme.
    [Fact]
    public void BottomTabLabelsStayReadableInTheDarkTheme() => StaHelper.RunOnSta(() =>
    {
        try
        {
            new ThemeService().ApplyTheme(ColorTheme.Dark);
            EnsureStyles();
            var tab = new TabItem { Style = (Style)Application.Current.Resources["EsTabView.FrmMainBottomItem"], Header = "Folders" };
            ((SolidColorBrush)tab.Foreground).Color.Should().Be(SystemColors.ControlTextColor);
        }
        finally
        {
            new ThemeService().ApplyTheme(ColorTheme.Light);
        }
    });
    // At larger interface sizes the source lists' rows fit the list: long titles end with an ellipsis, numbers are never cut,
    // and nothing runs under the scroll bar WPF-UI draws over the rows.
    [Theory]
    [InlineData("LibrarySongList", InterfaceSize.Large)]
    [InlineData("LibrarySongList", InterfaceSize.Senior)]
    [InlineData("PraiseBookItems", InterfaceSize.Large)]
    [InlineData("PraiseBookItems", InterfaceSize.Senior)]
    [InlineData("검색 결과 목록", InterfaceSize.Large)]
    [InlineData("검색 결과 목록", InterfaceSize.Senior)]
    [InlineData("제목 조회 결과 목록", InterfaceSize.Large)]
    [InlineData("제목 조회 결과 목록", InterfaceSize.Senior)]
    public void SourceListRowsFitTheListAtLargerSizes(string list, InterfaceSize size) => StaHelper.RunOnSta(() =>
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            EnsureStyles();
            const string longTitle = "갈보리산위에십자가섰으니주가흘린보배피로";
            var host = new ListHost();
            foreach (var (title, number) in Enumerable.Range(0, 30).Select(n => (n % 3 == 0 ? longTitle : "가나안의혼인잔치", $"{100 + n}(새)")))
            {
                host.Library.Songs.Add(new SongRow(title, number));
                host.Entries.Add(new PraiseRow(title, new[] { "7", "42", "315", "1204" }[host.Entries.Count % 4]));
                host.Search.SearchResults.Add(new FoundRow(title, "새찬송가 폴더 이름이 긴 폴더"));
                host.Search.LookupCandidates.Add(new FoundRow(title, "새찬송가 폴더 이름이 긴 폴더"));
            }
            var root = ParseWithWindowResources(e => Attr(e, "Name") == list || Attr(e, "AutomationProperties.Name") == list);
            root.DataContext = host;
            // As in the app: laid out at Standard in the default window, the interface size changed, then the window narrowed
            // to 1024x640's source pane. (A resource on the root, because the app's resource changes reach windows only.)
            var body = (double)Application.Current.Resources["Type.Body.FontSize"];
            root.Resources["Type.Body.FontSize"] = 11.0;
            Layout(root, new Size(290, 500));
            root.Resources["Type.Body.FontSize"] = body;
            Layout(root, new Size(290, 500));
            Layout(root, new Size(242, 500));
            var items = Descendants(root).OfType<ItemsControl>().First(c => c is ListBox);
            var bar = Descendants(items).OfType<ScrollBar>().FirstOrDefault(b => b.Orientation == Orientation.Vertical && Shown(b) && b.ActualWidth > 0);
            var listBox = Bounds(items, root);
            var right = bar != null ? Bounds(bar, root).Left : listBox.Right - items.BorderThickness.Right;
            var texts = Descendants(items).OfType<TextBlock>().Where(t => Shown(t) && t.Text.Length > 0 && t.ActualWidth > 0 && t.Ancestors<ListBoxItem>().Any()).ToArray();
            texts.Should().NotBeEmpty();
            foreach (var text in texts)
            {
                var box = Bounds(text, root);
                box.Right.Should().BeLessThanOrEqualTo(right + 0.5, $"{list} at {size}: \"{text.Text}\" ends inside the list, before the scroll bar ({box} vs {right}; {Diagnose(items)})");
                if (text.Text.Contains("(새)") || Regex.IsMatch(text.Text, "^[0-9]+$"))
                    TextWidth(text).Should().BeLessThanOrEqualTo(text.ActualWidth + 0.5, $"{list} at {size}: the number {text.Text} is shown whole");
            }
            var viewport = Descendants(items).OfType<ScrollViewer>().First().ViewportWidth;
            foreach (var row in Descendants(items).OfType<ListBoxItem>().Where(Shown))
                row.ActualWidth.Should().BeApproximately(viewport, 1, $"{list} at {size}: each row spans the list (numbers stay at its right edge)");
            if (list == "PraiseBookItems")
            {
                var numberEdges = texts.Where(t => Regex.IsMatch(t.Text, "^[0-9]+$")).Select(t => Math.Round(Bounds(t, root).Right, 1)).Distinct().ToArray();
                numberEdges.Should().ContainSingle($"{list} at {size}: numbers of any length line up at one right edge");
            }
            var trimmedTitle = texts.First(t => t.Text == longTitle);
            TextWidth(trimmedTitle).Should().BeGreaterThan(trimmedTitle.ActualWidth + 1, $"{list} at {size}: a long title ends with an ellipsis");
            trimmedTitle.TextTrimming.Should().Be(TextTrimming.CharacterEllipsis);
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    });

    private static string Diagnose(ItemsControl items)
    {
        var viewer = Descendants(items).OfType<ScrollViewer>().First();
        var presenter = Descendants(items).OfType<ScrollContentPresenter>().First();
        var panel = Descendants(items).OfType<VirtualizingStackPanel>().FirstOrDefault();
        var rows = Descendants(items).OfType<ListBoxItem>().Take(3).Select(r => $"{r.ActualWidth:0.#}/{r.DesiredSize.Width:0.#}");
        return $"list={items.ActualWidth:0.#} viewer={viewer.ActualWidth:0.#} viewport={viewer.ViewportWidth:0.#} extent={viewer.ExtentWidth:0.#} hbar={viewer.HorizontalScrollBarVisibility} canScroll={viewer.CanContentScroll} presenter={presenter.ActualWidth:0.#} panel={panel?.ActualWidth:0.#} rows={string.Join(",", rows)}";
    }
    // One element of MainWindow.xaml, parsed with the window's own resources and the WPF-UI theme the app loads; event
    // handlers and context menus are dropped (they need the compiled window).
    private static FrameworkElement ParseWithWindowResources(Func<XElement, bool> which)
    {
        var window = Load("MainWindow.xaml");
        var element = new XElement(window.Descendants().First(which));
        // Keep the window's prefixes on the detached copy, so it is not written out with its own default namespace.
        foreach (var declaration in window.Root!.Attributes().Where(a => a.IsNamespaceDeclaration))
            if (element.Attribute(declaration.Name) == null) element.Add(new XAttribute(declaration));
        element.Descendants().Where(e => e.Name.LocalName.EndsWith(".ContextMenu", StringComparison.Ordinal)).Remove();
        foreach (var attribute in element.DescendantsAndSelf().SelectMany(e => e.Attributes()).ToList())
            if (attribute.Name.LocalName is not ("Name" or "Key") && Regex.IsMatch(attribute.Value, "^[A-Za-z]+_[A-Za-z_]+$")) attribute.Remove();
        // The markup compiler reads an unprefixed attached property (Grid.Column on controls:FrmIcon) in the WPF namespace; the
        // loose reader takes the element's own, so name it explicitly.
        XNamespace wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        foreach (var owner in element.DescendantsAndSelf().Where(e => e.Name.Namespace != wpf).ToList())
            foreach (var attached in owner.Attributes().Where(a => a.Name.Namespace == XNamespace.None && a.Name.LocalName.Contains('.')).ToList())
            {
                attached.Remove();
                owner.Add(new XAttribute(wpf + attached.Name.LocalName, attached.Value));
            }
        var resources = string.Concat(window.Root!.Elements().Single(e => e.Name.LocalName == "Window.Resources").Elements().Select(e => e.ToString()));
        var prefixes = string.Join(" ", window.Root.Attributes().Where(a => a.IsNamespaceDeclaration).Select(a => a.ToString()));
        var xaml = $"<Grid {prefixes}><Grid.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries><ui:ThemesDictionary Theme=\"Light\" /><ui:ControlsDictionary />"
            + $"</ResourceDictionary.MergedDictionaries>{resources}</ResourceDictionary></Grid.Resources>{element}</Grid>";
        return (FrameworkElement)XamlReader.Parse(WithAssembly(xaml));
    }

    public sealed record SongRow(string Title, string Category)
    {
        public int SongNumber => 0;
    }

    public sealed class PanelHeightHost
    {
        public int DefaultPanelHeight { get; set; } = 100;
    }

    public sealed record PraiseRow(string Title, string Number);

    public sealed record FoundRow(string Title, string FolderName);

    public sealed class ListHost
    {
        public LibraryRows Library { get; } = new();
        public SearchRows Search { get; } = new();
        public ObservableCollection<PraiseRow> Entries { get; } = [];
        public object? SelectedSearchResult { get; set; }
        public object? SelectedTitleCandidate { get; set; }
    }

    public sealed class LibraryRows
    {
        public ObservableCollection<SongRow> Songs { get; } = [];
        public object? SelectedSong { get; set; }
        public bool UseSongNumbering { get; set; }
        public string StatusMessage { get; set; } = "";
    }

    public sealed class SearchRows
    {
        public ObservableCollection<FoundRow> SearchResults { get; } = [];
        public ObservableCollection<FoundRow> LookupCandidates { get; } = [];
    }
    // Tab strips other than the six source tabs (Bible versions, Worship List / Praise Book) keep one row that scrolls
    // sideways; the app's overlay scroll bar then sits below the labels, not over them.
    [Theory]
    [InlineData(InterfaceSize.Standard)]
    [InlineData(InterfaceSize.Senior)]
    public void OtherBottomTabStripsScrollInOneRowWithTheBarBelowTheLabels(InterfaceSize size) => StaHelper.RunOnSta(() =>
    {
        try
        {
            new ThemeService().ApplyInterfaceSize(size);
            EnsureStyles();
            var tabs = new TabControl { Style = (Style)Application.Current.Resources["EsTabView.FrmMainBottom"], TabStripPlacement = Dock.Bottom };
            foreach (var name in new[] { "개역개정", "개역한글", "새번역", "공동번역", "KJV", "NIV", "ESV", "NASB" })
                tabs.Items.Add(new TabItem { Header = name, Content = new Border() });
            tabs.SelectedIndex = 0;
            var host = new Grid { Children = { tabs } };
            host.Resources.MergedDictionaries.Add(new global::Wpf.Ui.Markup.ThemesDictionary { Theme = global::Wpf.Ui.Appearance.ApplicationTheme.Light });
            host.Resources.MergedDictionaries.Add(new global::Wpf.Ui.Markup.ControlsDictionary());
            Layout(host, new Size(190, 200));

            var items = tabs.Items.Cast<TabItem>().ToArray();
            items.Select(i => Math.Round(Bounds(i, host).Top)).Distinct().Should().ContainSingle($"{size}: the versions stay in one row");
            var bar = Descendants(tabs).OfType<ScrollBar>().Single(b => b.Orientation == Orientation.Horizontal);
            bar.Visibility.Should().Be(Visibility.Visible, "eight versions do not fit in 190, so the row scrolls");
            var barTop = Bounds(bar, host).Top;
            foreach (var item in items.Where(i => Bounds(i, host).Left < 190))
                Bounds(item, host).Bottom.Should().BeLessThanOrEqualTo(barTop + 0.5, $"{size}: the scroll bar stays below {item.Header} (bar {Bounds(bar, host)}, item {Bounds(item, host)} fs {item.FontSize}, panel {Bounds(Descendants(tabs).OfType<TabPanel>().Single(), host)} margin {Descendants(tabs).OfType<TabPanel>().Single().Margin}, viewer {Bounds(Descendants(tabs).OfType<ScrollViewer>().First(), host)} presenter {Bounds(Descendants(tabs).OfType<ScrollContentPresenter>().First(), host)})");
        }
        finally
        {
            new ThemeService().ApplyInterfaceSize(InterfaceSize.Standard);
        }
    });

    [Fact]
    public void OnlyTheSourceTabsWrap()
    {
        var window = Load("MainWindow.xaml");
        string StyleOf(string name) => Attr(window.Descendants().Single(e => Attr(e, "Name") == name), "Style");
        StyleOf("LeftBrowserTabs").Should().Be("{StaticResource EsTabView.FrmMainSourceTabs}");
        StyleOf("LeftListTabs").Should().Be("{StaticResource EsTabView.FrmMainBottom}");
        window.Descendants().Single(e => Attr(e, "Name") == "TabBibleVersions").Descendants()
            .Single(e => e.Name.LocalName == "Style" && Attr(e, "TargetType") == "TabControl").Attribute("BasedOn")!.Value
            .Should().Be("{StaticResource EsTabView.FrmMainBottom}");
        window.Descendants().Where(e => Attr(e, "SharedSizeGroup") != "").Should().BeEmpty("a shared size group in a virtualizing list re-measures every row while it scrolls");
    }
    // Boxes that hold body text take a minimum size, not a fixed one, so a larger interface size grows them.
    [Fact]
    public void TextBoxesInStripsHaveMinimumNotFixedSizes()
    {
        var window = Load("MainWindow.xaml");
        foreach (var name in new[] { "BookLookup", "BibleUserLookup", "Def_TransItem", "Def_TransSlides", "Def_PanelFontList" })
            Attr(window.Descendants().Single(e => Attr(e, "Name") == name), "Height").Should().BeEmpty(name);
        var menu = window.Descendants().Single(e => e.Name.LocalName == "Menu" && Attr(e, "AutomationProperties.Name") == "주 메뉴");
        menu.DescendantsAndSelf().Where(e => e.Name.LocalName is "Menu" or "MenuItem" && Attr(e, "Height") != "").Select(e => Attr(e, "Header")).Should().BeEmpty("the menu bar and its headers");
        foreach (var key in new[] { "ClassicTopToolStripButton", "ClassicTopToolStripComboBox", "DefaultColourButton" })
            window.Descendants().Single(e => e.Name.LocalName == "Style" && Attr(e, "Key") == key).Elements()
                .Where(e => Attr(e, "Property") == "Height").Should().BeEmpty(key);
        foreach (var name in new[] { "IndradioButtonText", "IndradioButtonFormat", "IndradioButtonInfo" })
            Attr(window.Descendants().Single(e => Attr(e, "Name") == name), "Width").Should().BeEmpty($"{name}'s label widens with the text");
    }
    private static ResourceDictionary WindowResources(XDocument window)
    {
        var resources = window.Root!.Elements().Single(e => e.Name.LocalName == "Window.Resources");
        var prefixes = string.Join(" ", window.Root.Attributes().Where(a => a.IsNamespaceDeclaration).Select(a => a.ToString()));
        var xaml = $"<ResourceDictionary {prefixes}>" + string.Concat(resources.Elements().Select(e => e.ToString())) + "</ResourceDictionary>";
        return (ResourceDictionary)XamlReader.Parse(WithAssembly(xaml));
    }

    private sealed class QueueHost(params LiveQueueItem[] items)
    {
        public ObservableCollection<LiveQueueItem> Queue { get; } = [.. items];

        public LiveQueueItem? LiveRow { get; init; }
    }
}
internal static class VisualAncestors
{
    public static IEnumerable<T> Ancestors<T>(this DependencyObject start) where T : DependencyObject
    {
        for (var at = VisualTreeHelper.GetParent(start); at != null; at = VisualTreeHelper.GetParent(at))
            if (at is T match) yield return match;
    }
}
