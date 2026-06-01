using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

/// <summary>
/// FrmMain 의 메뉴바(File/Edit/View/Output/Tools/Help, 52항목)를 현대적 WPF 메뉴로 포팅한 것을 잠근다.
/// 모든 메뉴 항목은 실제 명령(MainViewModel) 또는 DI 창 런처에 배선돼야 한다(dead UI 금지).
/// 구조 검증(프로젝트 표준 — MainWindow.xaml 텍스트 파싱). 핸들러 누락은 빌드가 잡고, 명령 존재는 VM 으로 보장.
/// </summary>
public class MainMenuBarTests
{
    private static string Xaml => File.ReadAllText(
        Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "MainWindow.xaml"),
        Encoding.UTF8);

    [Fact]
    public void MainWindow_HasMenuBarWithSixTopLevelMenus()
    {
        var xaml = Xaml;
        xaml.Should().Contain("<Menu", "FrmMain 메뉴바를 현대적 WPF Menu 로 포팅");
        // 6 대메뉴(한글 현대화) — FrmMain File/Edit/View/Output/Tools/Help 대응.
        foreach (var header in new[] { "파일", "편집", "보기", "출력", "도구", "도움말" })
        {
            xaml.Should().Contain($"Header=\"{header}\"", $"{header} 대메뉴가 있어야 함");
        }
    }

    [Theory]
    // 출력 메뉴 항목은 기존 MainViewModel 명령에 직접 배선(실동작 보장).
    [InlineData("GoLiveCommand")]
    [InlineData("StopLiveCommand")]
    [InlineData("BlackScreenCommand")]
    [InlineData("ClearOutputCommand")]
    [InlineData("HideOutputCommand")]
    [InlineData("RestoreOutputCommand")]
    [InlineData("RestartCurrentItemCommand")]
    [InlineData("RefreshOutputCommand")]
    [InlineData("NextItemCommand")]
    [InlineData("PreviousItemCommand")]
    public void MenuBar_WiresOutputItemsToRealCommands(string commandName)
        => Xaml.Should().Contain($"{{Binding {commandName}}}", $"{commandName} 메뉴 항목이 실제 명령에 배선돼야 함");

    [Theory]
    // 편집/도구/보기/도움말 항목은 DI 창 런처(Click 핸들러)에 배선. 핸들러 미정의면 빌드가 실패(컴파일 가드).
    [InlineData("SongCopy_Click")]
    [InlineData("SongMove_Click")]
    [InlineData("SongDelete_Click")]
    [InlineData("SongRecover_Click")]
    [InlineData("SongMerge_Click")]
    [InlineData("FolderEditor_Click")]
    [InlineData("OpenEasiSlidesFolder_Click")]
    [InlineData("OpenSettings_Click")]
    [InlineData("OpenImportExport_Click")]
    [InlineData("OpenSearchUsage_Click")]
    [InlineData("OpenExternalFiles_Click")]
    [InlineData("OpenManageWorshipLists_Click")]
    [InlineData("OpenBibleVersionManager_Click")]
    [InlineData("OpenHelp_Click")]
    [InlineData("OpenRegistration_Click")]
    [InlineData("OpenAbout_Click")]
    [InlineData("AddExternalFile_Click")]
    public void MenuBar_WiresWindowItemsToClickHandlers(string handler)
        => Xaml.Should().Contain($"Click=\"{handler}\"", $"{handler} 에 배선된 메뉴 항목이 있어야 함");

    [Fact]
    public void MenuBar_FileMenuHasExit()
        => Xaml.Should().Contain("Header=\"종료\"", "파일 메뉴에 종료가 있어야 함");

    [Fact]
    public void MenuBar_HasLyricsAlignmentSubmenu_WiredToAlignmentCommand()
    {
        var xaml = Xaml;
        xaml.Should().Contain("Header=\"가사 정렬\"", "가사 정렬 서브메뉴가 있어야 함");
        xaml.Should().Contain("{Binding ApplyLyricsAlignmentCommand}", "정렬 명령에 배선");
        foreach (var member in new[] { "Left", "Center", "Right" })
        {
            xaml.Should().Contain($"{{x:Static settings:LyricsTextAlignment.{member}}}", $"{member} 정렬 파라미터");
        }
    }

    [Theory]
    // 출력 표시 토글(FrmMain "Show …") — 기존 상태(Active*)에 IsChecked 단방향 + 토글 명령 배선.
    [InlineData("ActiveLyricsPositionIndicator", "ToggleLyricsPositionIndicatorCommand")]
    [InlineData("ActiveLyricsTitleHeading", "ToggleLyricsTitleHeadingCommand")]
    [InlineData("ActiveLyricsOutline", "ToggleLyricsOutlineCommand")]
    [InlineData("ActiveLyricsShadow", "ToggleLyricsShadowCommand")]
    [InlineData("ActiveLyricsBold", "ToggleLyricsBoldCommand")]
    [InlineData("ActiveLyricsItalic", "ToggleLyricsItalicCommand")]
    public void MenuBar_OutputDisplayToggles_BindCheckedAndCommand(string activeProperty, string toggleCommand)
    {
        var xaml = Xaml;
        xaml.Should().Contain($"IsChecked=\"{{Binding {activeProperty}}}\"", $"{activeProperty} 체크 상태 바인딩");
        xaml.Should().Contain($"Command=\"{{Binding {toggleCommand}}}\"", $"{toggleCommand} 토글 명령 배선");
    }

    [Theory]
    // FrmMain 라이브 운영 단축키를 메뉴에 힌트로 노출(발견가능성 — 현대적 UX). 실제 키 배선은 CommandCatalog.
    [InlineData("InputGestureText=\"F12\"")]  // Go LIVE
    [InlineData("InputGestureText=\"F9\"")]   // 검은 화면
    [InlineData("InputGestureText=\"F3\"")]   // 화면 비우기
    [InlineData("InputGestureText=\"F1\"")]   // 도움말
    public void MenuBar_ShowsFunctionKeyGestureHints(string gesture)
        => Xaml.Should().Contain(gesture, $"메뉴에 {gesture} 단축키 힌트가 보여야 함");

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
