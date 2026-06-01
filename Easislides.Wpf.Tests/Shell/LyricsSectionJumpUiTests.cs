using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 절 라벨 직접 점프 버튼 행(레거시 FrmInfoScreen 절 버튼 1~9·c·b 대응)이 Preview 탭에 배선됐는지 잠근다.
/// AvailableSectionLabels 를 ItemsSource 로, JumpToLyricsSectionCommand 를 라벨 파라미터로 호출하는 구조를 검증.
/// </summary>
public class LyricsSectionJumpUiTests
{
    private static string Xaml => File.ReadAllText(
        Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "MainWindow.xaml"),
        Encoding.UTF8);

    [Fact]
    public void Preview_HasSectionJumpBarBoundToAvailableSectionLabels()
    {
        var xaml = Xaml;
        xaml.Should().Contain("x:Name=\"SectionJumpBar\"", "절 라벨 점프 바가 있어야 함");
        xaml.Should().Contain("ItemsSource=\"{Binding AvailableSectionLabels}\"", "라벨 목록에 바인딩");
    }

    [Fact]
    public void SectionJumpBar_InvokesJumpCommandWithLabelParameter()
    {
        var xaml = Xaml;
        xaml.Should().Contain("JumpToLyricsSectionCommand", "라벨 버튼이 절 점프 명령에 배선");
        xaml.Should().Contain("CommandParameter=\"{Binding}\"", "라벨 자체를 점프 파라미터로 전달");
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
