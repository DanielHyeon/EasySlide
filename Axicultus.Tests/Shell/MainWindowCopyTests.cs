using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public class MainWindowCopyTests
{
    [Fact]
    public void MainWindowXaml_UsesFrmMainWindowChromeAndReadableOperationalLabels()
    {
        var xaml = File.ReadAllText(
            Path.Combine(FindRepositoryRoot(), "Axicultus", "MainWindow.xaml"),
            Encoding.UTF8);

        xaml.Should().Contain("Title=\"Axicultus\"");
        foreach (var group in new[] { "File", "Edit", "View", "Output", "Tools", "Help" })
            xaml.Should().Contain($"Header=\"{{loc:MenuText Menu_{group}}}\""); // in the UI language (MenuTextTests)
        xaml.Should().NotContain("Axicultus v3.0 - WPF 운영 셸");
        xaml.Should().Contain("출력 열기");
        xaml.Should().Contain("출력 닫기");
        xaml.Should().Contain("설정");
        xaml.Should().Contain("예배 순서");
        xaml.Should().Contain("현재 항목");
        xaml.Should().Contain("운영 상태");
        xaml.Should().Contain("송출");
        xaml.Should().Contain("선택");
        xaml.Should().Contain("이전");
        xaml.Should().Contain("다음");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Axicultus.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to find repository root.");
    }
}
