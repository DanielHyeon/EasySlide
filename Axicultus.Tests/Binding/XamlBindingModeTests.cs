using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Axicultus.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Binding;

public sealed class XamlBindingModeTests
{
    [Fact]
    public void ProductionXaml_IsCheckedBindings_DeclareBindingMode()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var offenders = XamlAccessibilityScanner.ProductionXamlFiles(repoRoot)
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new
                {
                    Path = Path.GetRelativePath(repoRoot, path).Replace('\\', '/'),
                    Line = index + 1,
                    Text = line.Trim(),
                }))
            .Where(x => Regex.IsMatch(x.Text, "IsChecked=\"\\{Binding [^,}\\\"]+\\}\""))
            .Select(x => $"{x.Path}:{x.Line}: {x.Text}")
            .ToList();

        offenders.Should().BeEmpty(
            "IsChecked defaults to TwoWay and throws when bound to read-only computed state properties");
    }

    // The About and Registration windows both crashed the app on open this way: a display-only TextBox bound
    // (TwoWay by default) to a get-only view-model property.
    [Fact]
    public void ProductionXaml_ReadOnlyTextBoxes_BindTextOneWay()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var offenders = XamlAccessibilityScanner.ProductionXamlFiles(repoRoot)
            .SelectMany(path => Regex.Matches(File.ReadAllText(path), @"<TextBox\b[^>]*>")
                .Where(tag => tag.Value.Contains("IsReadOnly=\"True\"")
                    && Regex.IsMatch(tag.Value, "\\bText=\"\\{Binding (?![^\"]*Mode=)[^\"]*\\}\""))
                .Select(tag => $"{Path.GetRelativePath(repoRoot, path).Replace('\\', '/')}: {Regex.Replace(tag.Value, @"\s+", " ")}"))
            .ToList();

        offenders.Should().BeEmpty("TextBox.Text binds TwoWay by default and WPF throws when the source property has no setter");
    }

    // A missing key throws XamlParseException from the window constructor (Import/Export did, with Type.H2.Style).
    [Fact]
    public void ProductionXaml_StaticResourceKeys_AreDefined()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var files = XamlAccessibilityScanner.ProductionXamlFiles(repoRoot).Select(path => (Path: path, Text: File.ReadAllText(path))).ToList();
        var defined = files.SelectMany(file => Regex.Matches(file.Text, "x:Key=\"([^\"]+)\"").Select(m => m.Groups[1].Value)).ToHashSet();
        var missing = files
            .SelectMany(file => Regex.Matches(file.Text, @"\{StaticResource\s+(?:ResourceKey=)?([^}\s,]+)\}")
                .Select(m => m.Groups[1].Value)
                .Where(key => !defined.Contains(key))
                .Select(key => $"{Path.GetRelativePath(repoRoot, file.Path).Replace('\\', '/')}: {key}"))
            .Distinct()
            .ToList();

        missing.Should().BeEmpty("every StaticResource used by the app must be declared in the app's own XAML");
    }
}
