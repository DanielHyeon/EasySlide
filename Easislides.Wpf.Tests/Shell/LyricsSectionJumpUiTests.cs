using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 절 라벨 직접 점프 버튼 행(레거시 FrmMain Preview 절 버튼 1~9·c·b 대응)이 Preview 하단 조작 스트립에 배선됐는지 잠근다.
/// </summary>
public class LyricsSectionJumpUiTests
{
    private static string Xaml => File.ReadAllText(
        Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "MainWindow.xaml"),
        Encoding.UTF8);

    [Fact]
    public void Preview_HasFrmMainFixedSectionJumpButtons()
    {
        var xaml = Xaml;
        xaml.Should().Contain("x:Name=\"PreviewBtnVerse1\"", "FrmMain fixed verse 1 button should be present");
        xaml.Should().Contain("x:Name=\"PreviewBtnVerseChorus\"", "FrmMain fixed chorus button should be present");
        xaml.Should().Contain("x:Name=\"PreviewBtnVerseBridge\"", "FrmMain fixed bridge button should be present");
        xaml.Should().NotContain("x:Name=\"SectionJumpBar\"",
            "FrmMain keeps fixed verse buttons in the lower Preview operator strip, not a generated WPF-only jump bar");
    }

    [Fact]
    public void PreviewFixedSectionButtons_InvokeJumpCommandWithLegacyParameters()
    {
        var xaml = Xaml;
        xaml.Should().Contain("x:Name=\"PreviewBtnVerse1\" Tag=\"1\" Command=\"{Binding JumpToLyricsSectionCommand}\" CommandParameter=\"1\"",
            "verse 1 should use FrmMain's fixed jump parameter");
        xaml.Should().Contain("x:Name=\"PreviewBtnVerseChorus\" Tag=\"0\" Command=\"{Binding JumpToLyricsSectionCommand}\" CommandParameter=\"c\"",
            "chorus should use FrmMain's fixed jump parameter");
        xaml.Should().Contain("x:Name=\"PreviewBtnVerseBridge\" Tag=\"100\" Command=\"{Binding JumpToLyricsSectionCommand}\" CommandParameter=\"b\"",
            "bridge should use FrmMain's fixed jump parameter");
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
