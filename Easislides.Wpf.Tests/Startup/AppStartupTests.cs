using System.IO;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Startup;

public sealed class AppStartupTests
{
    [Fact]
    public void Startup_DefersAutomaticShutdown_UntilMainWindowIsAssigned()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var code = File.ReadAllText(Path.Combine(repoRoot, "Easislides.Wpf/App.xaml.cs"));

        code.Should().Contain("ShutdownMode = ShutdownMode.OnExplicitShutdown;",
            "output/preview host initialization can create and close transient windows before MainWindow is shown");
        code.Should().Contain("MainWindow = window;",
            "WPF should know which operator window owns the application lifetime");
        code.Should().Contain("ShutdownMode = ShutdownMode.OnMainWindowClose;",
            "after MainWindow is assigned, closing the operator console should still exit normally");

        var explicitIndex = code.IndexOf("ShutdownMode = ShutdownMode.OnExplicitShutdown;", System.StringComparison.Ordinal);
        var mainWindowIndex = code.IndexOf("MainWindow = window;", System.StringComparison.Ordinal);
        var mainWindowCloseIndex = code.IndexOf("ShutdownMode = ShutdownMode.OnMainWindowClose;", System.StringComparison.Ordinal);
        explicitIndex.Should().BeLessThan(mainWindowIndex);
        mainWindowIndex.Should().BeLessThan(mainWindowCloseIndex);
    }
}
