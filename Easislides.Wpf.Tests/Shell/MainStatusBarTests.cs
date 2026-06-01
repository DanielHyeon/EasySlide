using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// 하단 상태바(FrmMain StatusStrip 파리티)가 예배 순서 수·절 위치·운영 상태·출력 모니터를
/// 기존 VM 상태에 바인딩해 노출하는지 잠근다(구조 검증 — 프로젝트 표준).
/// </summary>
public class MainStatusBarTests
{
    private static string Xaml => File.ReadAllText(
        Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "MainWindow.xaml"),
        Encoding.UTF8);

    [Fact]
    public void MainWindow_HasStatusBar()
        => Xaml.Should().Contain("x:Name=\"StatusBar\"", "하단 상태바가 있어야 함");

    [Theory]
    [InlineData("{Binding Queue.Count, StringFormat='예배 순서 {0}개'}")] // 예배 순서 항목 수
    [InlineData("Text=\"{Binding LyricsPageLabel}\"")]                    // 현재 절 위치
    [InlineData("Text=\"{Binding StatusText}\"")]                        // 운영 상태
    [InlineData("Text=\"{Binding SelectedOutputDisplay.Name}\"")]        // 출력 모니터
    public void StatusBar_BindsExistingLiveState(string binding)
        => Xaml.Should().Contain(binding, $"상태바가 {binding} 를 노출해야 함");

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
