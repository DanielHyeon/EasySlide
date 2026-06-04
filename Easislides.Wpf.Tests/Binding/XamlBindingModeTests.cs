using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Binding;

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
}
