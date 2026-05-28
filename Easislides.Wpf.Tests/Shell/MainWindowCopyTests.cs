using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public class MainWindowCopyTests
{
    [Fact]
    public void MainWindowXaml_UsesReadableKoreanOperationalLabels()
    {
        var xaml = File.ReadAllText(
            Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "MainWindow.xaml"),
            Encoding.UTF8);

        xaml.Should().Contain("EasiSlides v3.0 - WPF 운영 셸");
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
            if (File.Exists(Path.Combine(directory.FullName, "Easislides.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to find repository root.");
    }
}
