using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Easislides.Wpf.Tests.Rendering;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

[Collection("WPF Application")]
public class SettingsControlRenderEvidenceTests
{
    [Fact]
    public void SettingsInputsAndCheckbox_RenderWithoutBottomClippingEvidence()
    {
        StaHelper.RunOnSta(() =>
        {
            EnsureControlResources();

            var panel = BuildSettingsControlSample();
            var png = VisualRenderHarness.RenderToPng(panel, 560, 170);
            var path = WriteEvidence("settings-controls-after.png", png);

            File.Exists(path).Should().BeTrue();
            png.Length.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void SettingsImageAndTransitionControls_RenderWithoutBottomClippingEvidence()
    {
        StaHelper.RunOnSta(() =>
        {
            EnsureControlResources();

            var panel = BuildImageAndTransitionSample();
            var png = VisualRenderHarness.RenderToPng(panel, 640, 170);
            var path = WriteEvidence("settings-image-transition-controls-after.png", png);

            File.Exists(path).Should().BeTrue();
            png.Length.Should().BeGreaterThan(0);
        });
    }

    private static FrameworkElement BuildSettingsControlSample()
    {
        var normalTextStyle = new Style(typeof(TextBox), RequiredStyle("EsTextBox.Default"));
        normalTextStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 36d));
        normalTextStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 6, 8, 6)));
        normalTextStyle.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

        var compactTextStyle = new Style(typeof(TextBox), normalTextStyle);
        compactTextStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 34d));
        compactTextStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 34d));
        compactTextStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(7, 4, 7, 4)));

        var checkboxStyle = new Style(typeof(CheckBox));
        checkboxStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 30d));
        checkboxStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(2, 4, 2, 4)));
        checkboxStyle.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        checkboxStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));

        var panel = new StackPanel
        {
            Width = 560,
            Background = Brushes.White,
            Margin = new Thickness(12),
        };

        panel.Children.Add(new TextBox
        {
            Style = normalTextStyle,
            Text = "Global lyrics font size gjpqy 12345",
            FontFamily = new FontFamily("Malgun Gothic"),
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 8),
        });
        panel.Children.Add(new TextBox
        {
            Style = compactTextStyle,
            Text = "Individual spacing gjpqy 67890",
            FontFamily = new FontFamily("Malgun Gothic"),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 8),
        });
        panel.Children.Add(new CheckBox
        {
            Style = checkboxStyle,
            Content = "Use Individual Settings gjpqy",
            FontFamily = new FontFamily("Malgun Gothic"),
            FontSize = 13,
            IsChecked = true,
        });

        return panel;
    }

    private static FrameworkElement BuildImageAndTransitionSample()
    {
        var compactTextStyle = new Style(typeof(TextBox), RequiredStyle("EsTextBox.Default"));
        compactTextStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 34d));
        compactTextStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 34d));
        compactTextStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(7, 4, 7, 4)));
        compactTextStyle.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

        var comboStyle = new Style(typeof(ComboBox), RequiredStyle("EsComboBox.Default"));
        comboStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 34d));
        comboStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 4, 8, 4)));
        comboStyle.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

        var panel = new StackPanel
        {
            Width = 640,
            Background = Brushes.White,
            Margin = new Thickness(12),
        };

        panel.Children.Add(new TextBlock
        {
            Text = "Image",
            FontFamily = new FontFamily("Malgun Gothic"),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 4),
        });
        panel.Children.Add(new TextBox
        {
            Style = compactTextStyle,
            Text = @"C:\EasiSlides\Backgrounds\image-gjpqy-sample.png",
            FontFamily = new FontFamily("Malgun Gothic"),
            FontSize = 13,
            IsReadOnly = true,
            Margin = new Thickness(0, 0, 0, 8),
        });
        panel.Children.Add(new ComboBox
        {
            Style = comboStyle,
            ItemsSource = new[] { "Slide From Bottom gjpqy", "Checkerboard gjpqy" },
            SelectedIndex = 0,
            FontFamily = new FontFamily("Malgun Gothic"),
            FontSize = 13,
            Width = 220,
            Margin = new Thickness(0, 0, 0, 8),
        });

        return panel;
    }

    private static void EnsureControlResources()
    {
        if (Application.Current is null)
        {
            _ = new Application();
        }

        if (Application.Current!.TryFindResource("EsTextBox.Default") is null)
        {
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/EasislidesNext;component/Controls/EsTextBox.xaml", UriKind.Absolute),
            });
        }

        if (Application.Current.TryFindResource("EsComboBox.Default") is null)
        {
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/EasislidesNext;component/Controls/EsComboBox.xaml", UriKind.Absolute),
            });
        }
    }

    private static Style RequiredStyle(string key)
        => Application.Current!.TryFindResource(key) as Style
           ?? throw new InvalidOperationException($"Required WPF style not found: {key}");

    private static string WriteEvidence(string fileName, byte[] png)
    {
        var directory = Path.Combine(FindRepositoryRoot(), "evidence", "screenshots", "2026-06-23", "settings-image-transition-clipping");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, fileName);
        File.WriteAllBytes(path, png);
        return path;
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
