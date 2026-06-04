using System;
using System.IO;
using System.Linq;
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

    private static string CodeBehind => File.ReadAllText(
        Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "MainWindow.xaml.cs"),
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
    [InlineData("GoLiveCommand")]
    [InlineData("SendToOutputAndNextCommand")]
    [InlineData("StopLiveCommand")]
    [InlineData("BlackScreenCommand")]
    [InlineData("ClearOutputCommand")]
    [InlineData("HideOutputCommand")]
    [InlineData("RestoreOutputCommand")]
    [InlineData("RestartCurrentItemCommand")]
    [InlineData("RefreshOutputCommand")]
    [InlineData("OpenOutputCommand")]
    [InlineData("CloseOutputCommand")]
    public void OperatorBar_ExposesCoreLiveCommands(string commandName)
        => OperatorBarXaml.Should().Contain($"{{Binding {commandName}}}", $"{commandName} 이 메뉴가 아니라 첫 화면 고정 바에 있어야 함");

    [Fact]
    public void OperatorBar_KeepsStopLiveAsDangerActionBetweenSendAndBlack()
    {
        var operatorBar = OperatorBarXaml;
        var sendIndex = operatorBar.IndexOf("Command=\"{Binding SendToOutputAndNextCommand}\"", StringComparison.Ordinal);
        var stopIndex = operatorBar.IndexOf("Command=\"{Binding StopLiveCommand}\"", StringComparison.Ordinal);
        var blackIndex = operatorBar.IndexOf("Command=\"{Binding BlackScreenCommand}\"", StringComparison.Ordinal);
        var closeIndex = operatorBar.IndexOf("Command=\"{Binding CloseOutputCommand}\"", StringComparison.Ordinal);

        sendIndex.Should().BeGreaterThanOrEqualTo(0, "Send-and-next must stay in the fixed operator bar");
        stopIndex.Should().BeGreaterThan(sendIndex, "Stop Live should stay beside send-and-next as the immediate stop action");
        blackIndex.Should().BeGreaterThan(stopIndex, "Stop Live should remain before the other live safety actions");
        closeIndex.Should().BeGreaterThan(stopIndex, "Stop Live must remain distinct from Close Output");

        var stopButton = ButtonBlockFor(operatorBar, "StopLiveCommand");
        stopButton.Should().Contain("Style=\"{StaticResource EsButton.Danger}\"", "Stop Live is a dangerous live action");
        stopButton.Should().NotContain("CloseOutputCommand", "Stop Live must not drift into the Close Output button");
    }

    [Fact]
    public void MainWindow_Loaded_PreloadsBibleOnce()
    {
        var code = CodeBehind;
        code.Should().Contain("EnsureBibleLoadedOnce();", "Bible data must be ready from the main shell startup");
        code.Should().Contain("private void EnsureBibleLoadedOnce()", "Bible startup load should use the same one-shot guard as tab selection");
    }

    [Fact]
    public void MainWindow_UsesFrmMainStyleSourcePreviewOutputPanes()
    {
        var xaml = Xaml;

        foreach (var pane in new[]
        {
            "ClassicFrmMainConsole",
            "ClassicSourcePane",
            "ClassicPreviewPane",
            "ClassicPreviewSlidePane",
            "ClassicOutputPane",
            "ClassicOutputSlidePane",
        })
        {
            xaml.Should().Contain($"x:Name=\"{pane}\"", $"{pane} keeps the FrmMain source/preview/output geometry discoverable");
        }

        xaml.Should().Contain("Grid.Column=\"4\"", "Output should occupy the right-side FrmMain column, not the old inspector slot");
        xaml.Should().Contain("Grid.Row=\"2\"", "Preview and Output should both have lower slide surfaces like FrmMain");
    }

    [Fact]
    public void ClassicOutputPane_ShowsPowerPointThumbnailsAndLargeOutputSlide()
    {
        var xaml = Xaml;

        xaml.Should().Contain("x:Name=\"ClassicOutputThumbnailGrid\"", "right-top Output pane should match FrmMain PPT thumbnail mode");
        xaml.Should().Contain("ItemsSource=\"{Binding PowerPoint.Thumbnails}\"", "Output thumbnails must use the PPT thumbnail source");
        xaml.Should().Contain("Command=\"{Binding DataContext.GoToSlideCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}\"",
            "thumbnail clicks should navigate slides");
        xaml.Should().Contain("x:Name=\"ClassicOutputLargeSlideImage\"", "right-bottom Output pane should expose the large slide surface");
        xaml.Should().Contain("Source=\"{Binding PowerPoint.PreviewImage}\"", "large Output slide should show the current PPT preview image when available");
    }

    [Fact]
    public void LeftBrowserTabs_ExposeInlinePowerPointAndMediaSources()
    {
        var xaml = Xaml;

        xaml.Should().Contain("Tag=\"PowerPointSource\"", "FrmMain keeps PowerPoint as a main-console source tab");
        xaml.Should().Contain("x:Name=\"PowerPointSourceTab\"", "inline PowerPoint tab needs a stable DataContext target");
        xaml.Should().Contain("x:Name=\"InlinePowerPointList\"", "PowerPoint files must be visible without opening a modal window");
        xaml.Should().Contain("ItemsSource=\"{Binding Presentations}\"", "inline PowerPoint list reuses PowerPointLibraryViewModel");
        xaml.Should().Contain("MouseDoubleClick=\"InlinePowerPointList_MouseDoubleClick\"", "double-click should add selected PowerPoint");
        xaml.Should().Contain("PreviewMouseMove=\"InlinePowerPointList_PreviewMouseMove\"", "PowerPoint rows should drag into the Worship List");

        xaml.Should().Contain("Tag=\"MediaSource\"", "FrmMain keeps Media as a main-console source tab");
        xaml.Should().Contain("x:Name=\"MediaSourceTab\"", "inline Media tab needs a stable DataContext target");
        xaml.Should().Contain("x:Name=\"InlineMediaList\"", "Media files must be visible without opening a modal window");
        xaml.Should().Contain("ItemsSource=\"{Binding MediaFiles}\"", "inline Media list reuses MediaLibraryViewModel");
        xaml.Should().Contain("MouseDoubleClick=\"InlineMediaList_MouseDoubleClick\"", "double-click should add selected Media");
        xaml.Should().Contain("PreviewMouseMove=\"InlineMediaList_PreviewMouseMove\"", "Media rows should drag into the Worship List");
    }

    [Fact]
    public void InlinePowerPointAndMediaSources_LazyLoadAndUseFileDropContract()
    {
        var code = CodeBehind;

        code.Should().Contain("EnsureInlinePowerPointLoadedOnce(viewModel)", "PowerPoint source tab should lazy-load on first selection");
        code.Should().Contain("EnsureInlineMediaLoadedOnce(viewModel)", "Media source tab should lazy-load on first selection");
        code.Should().Contain("PowerPointSourceTab.DataContext = _inlinePowerPoint", "inline PowerPoint tab should bind to its library VM");
        code.Should().Contain("MediaSourceTab.DataContext = _inlineMedia", "inline Media tab should bind to its library VM");
        code.Should().Contain("DataFormats.FileDrop", "source drags must reuse the WorshipListPanel external file drop contract");
        code.Should().Contain("Path.Combine(workingFolder, \"Powerpoint\")", "PowerPoint should prefer the legacy working-folder source directory");
        code.Should().Contain("Path.Combine(workingFolder, \"Media\")", "Media should prefer the legacy working-folder source directory");
    }

    [Fact]
    public void ClassicPreviewAndOutputCommandStrips_StayAttachedToTheirPanes()
    {
        var xaml = Xaml;

        foreach (var strip in new[]
        {
            "ClassicPreviewTopStrip",
            "ClassicPreviewBottomStrip",
            "ClassicOutputTopStrip",
            "ClassicOutputBottomStrip",
        })
        {
            xaml.Should().Contain($"x:Name=\"{strip}\"", $"{strip} should stay attached to the local Preview/Output pane");
        }

        xaml.Should().Contain("{Binding SendToOutputAndNextCommand}", "Preview strip exposes send-and-next");
        xaml.Should().Contain("{Binding BlackScreenCommand}", "Output strip exposes Black");
        xaml.Should().Contain("{Binding ClearOutputCommand}", "Output strip exposes Clear");
        xaml.Should().Contain("{Binding RestoreOutputCommand}", "Output strip exposes Restore");
    }

    private static string OperatorBarXaml
    {
        get
        {
            var xaml = Xaml;
            var marker = xaml.IndexOf("x:Name=\"ClassicOperatorBar\"", StringComparison.Ordinal);
            marker.Should().BeGreaterThanOrEqualTo(0, "the fixed operator bar should have a stable XAML name for drift tests");

            var start = xaml.LastIndexOf("<WrapPanel", marker, StringComparison.Ordinal);
            start.Should().BeGreaterThanOrEqualTo(0, "ClassicOperatorBar should be a WrapPanel");

            var end = xaml.IndexOf("</WrapPanel>", marker, StringComparison.Ordinal);
            end.Should().BeGreaterThan(marker, "the fixed operator bar WrapPanel range should be discoverable");

            return xaml[start..end];
        }
    }

    private static string ButtonBlockFor(string container, string commandName)
    {
        var command = $"Command=\"{{Binding {commandName}}}\"";
        var marker = container.IndexOf(command, StringComparison.Ordinal);
        marker.Should().BeGreaterThanOrEqualTo(0, $"{commandName} should be in the fixed operator bar");

        var start = container.LastIndexOf("<Button", marker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0, $"{commandName} should be on a Button");

        var end = container.IndexOf("</Button>", marker, StringComparison.Ordinal);
        end.Should().BeGreaterThan(marker, $"{commandName} Button should be closed");

        return container[start..(end + "</Button>".Length)];
    }

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
    [InlineData("ImportLegacyWorshipList_Click")]
    public void MenuBar_WiresWindowItemsToClickHandlers(string handler)
        => Xaml.Should().Contain($"Click=\"{handler}\"", $"{handler} 에 배선된 메뉴 항목이 있어야 함");

    [Fact]
    public void MenuBar_FileMenuHasExit()
        => Xaml.Should().Contain("Header=\"종료\"", "파일 메뉴에 종료가 있어야 함");

    [Theory]
    // 미디어 전송 컨트롤이 텍스트 글리프 대신 Fluent SymbolIcon(EsIcons.Media*)으로 현대화됐는지(아이콘 업그레이드).
    [InlineData("theme:EsIcons.MediaRewind")]
    [InlineData("theme:EsIcons.MediaRestart")]
    [InlineData("theme:EsIcons.MediaStop")]
    [InlineData("theme:EsIcons.MediaFastForward")]
    [InlineData("theme:EsIcons.MediaRepeat")]
    public void MediaControls_UseFluentSymbolIcons(string symbol)
        => Xaml.Should().Contain(symbol, $"미디어 컨트롤이 {symbol} 아이콘을 써야 함(텍스트 글리프 아님)");

    [Theory]
    // 상태에 따라 바뀌는 아이콘은 VM 심볼 프로퍼티에 바인딩(재생↔일시정지, 음소거↔소리켜짐).
    [InlineData("Symbol=\"{Binding Media.PlayPauseSymbol}\"")] // 재생/일시정지
    [InlineData("Symbol=\"{Binding Media.MuteSymbol}\"")]      // 음소거/소리켜짐
    public void MediaControls_BindStateDrivenSymbols(string binding)
        => Xaml.Should().Contain(binding, $"{binding} 상태 구동 아이콘 바인딩이 있어야 함");

    [Theory]
    // 아이콘 버튼은 스크린리더가 읽을 한국어 접근성 이름이 반드시 남아 있어야 한다(아이콘만 버튼의 전제).
    [InlineData("되감기")]
    [InlineData("재생/일시정지")]
    [InlineData("처음부터 다시 재생")]
    [InlineData("정지")]
    [InlineData("빨리감기")]
    [InlineData("음소거 토글")]
    [InlineData("반복 토글")]
    public void MediaControls_KeepAccessibleNames(string name)
        => Xaml.Should().Contain($"AutomationProperties.Name=\"{name}\"", $"{name} 버튼의 접근성 이름이 남아 있어야 함");

    [Fact]
    public void MediaControls_DoNotUseLegacyTextGlyphs()
    {
        // 옛 텍스트 글리프(◀◀ ▶▶ ■ ↺)가 더는 Content 로 남아 있지 않아야 한다(현대 아이콘으로 교체 확인).
        var xaml = Xaml;
        foreach (var glyph in new[] { "Content=\"◀◀\"", "Content=\"▶▶\"", "Content=\"■\"", "Content=\"↺\"" })
        {
            xaml.Should().NotContain(glyph, $"옛 글리프 {glyph} 는 Fluent 아이콘으로 교체됐어야 함");
        }
    }

    [Theory]
    [InlineData("ApplyLyricsVerticalAlignmentCommand")] // 세로 정렬
    [InlineData("IncreaseLyricsFontSizeCommand")]       // 글자 크게
    [InlineData("DecreaseLyricsFontSizeCommand")]       // 글자 작게
    [InlineData("IncreaseLyricsLineSpacingCommand")]    // 줄 간격 늘림
    [InlineData("DecreaseLyricsLineSpacingCommand")]    // 줄 간격 줄임
    public void MenuBar_OutputFormattingExtras_WiredToCommands(string command)
        => Xaml.Should().Contain($"{{Binding {command}}}", $"{command} 메뉴 배선");

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
        xaml.Should().Contain($"IsChecked=\"{{Binding {activeProperty}, Mode=OneWay}}\"", $"{activeProperty} 체크 상태 바인딩");
        xaml.Should().Contain($"Command=\"{{Binding {toggleCommand}}}\"", $"{toggleCommand} 토글 명령 배선");
    }

    [Theory]
    // FrmMain 라이브 운영 단축키를 메뉴에 힌트로 노출(발견가능성 — 현대적 UX). 실제 키 배선은 CommandCatalog.
    [InlineData("InputGestureText=\"F12\"")]     // Go LIVE
    [InlineData("InputGestureText=\"F11\"")]     // 송출 후 다음 항목(증분135)
    [InlineData("InputGestureText=\"Space\"")]   // 다음 항목
    [InlineData("InputGestureText=\"Shift+Space\"")] // 이전 항목
    [InlineData("InputGestureText=\"F9\"")]      // 검은 화면
    [InlineData("InputGestureText=\"F3\"")]      // 화면 비우기
    [InlineData("InputGestureText=\"F1\"")]      // 도움말
    [InlineData("InputGestureText=\"Ctrl+R\"")]  // 현재 항목 처음으로(증분136)
    [InlineData("InputGestureText=\"Ctrl+F5\"")] // 출력 새로고침(증분136)
    public void MenuBar_ShowsCoreGestureHints(string gesture)
        => Xaml.Should().Contain(gesture, $"메뉴에 {gesture} 단축키 힌트가 보여야 함");

    [Theory]
    // 메뉴 힌트 문자열이 실제 카탈로그 단축키와 같은 메뉴 항목의 VM 명령에 일치해야 한다(거짓 힌트·잘못된 항목 부착 방지).
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveGo, "F12", "GoLiveCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveGoAndNext, "F11", "SendToOutputAndNextCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveNext, "Space", "NextItemCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LivePrevious, "Shift+Space", "PreviousItemCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveBlack, "F9", "BlackScreenCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveClear, "F3", "ClearOutputCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveRestart, "Ctrl+R", "RestartCurrentItemCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveRefresh, "Ctrl+F5", "RefreshOutputCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.WindowHelp, "F1", "OpenHelp_Click")]
    public void MenuGestureHints_MatchActualCatalogShortcutAndMenuCommand(
        string commandId,
        string gesture,
        string menuCommandOrClick)
    {
        // 카탈로그에 그 명령의 기본 단축키가 있고 표시 문자열이 메뉴 힌트와 같은지.
        var shortcuts = new Easislides.Wpf.Input.CommandCatalog()
            .GetDefaultShortcuts()
            .Where(s => s.CommandName == commandId)
            .ToList();
        var shortcut = shortcuts.FirstOrDefault(s => s.DisplayText == gesture);
        shortcut.Should().NotBeNull($"{commandId} 에 기본 단축키가 있어야 함");

        // 그리고 메뉴 XAML 에 그 힌트가 실제로 노출돼 있는지.
        var menuItem = MenuItemOpeningFor(menuCommandOrClick);
        menuItem.Should().Contain($"InputGestureText=\"{gesture}\"", $"{commandId} 메뉴에 {gesture} 힌트 노출");
    }

    [Fact]
    public void AllMenuGestureHints_AreRealCatalogShortcuts()
    {
        // 메뉴에 적힌 모든 InputGestureText 가 실제 카탈로그 단축키의 표시 문자열이어야 한다 — 존재하지 않는 가짜 힌트 방지.
        // (메뉴 힌트는 하드코딩 문자열이라, 카탈로그에 없는 키를 적어도 빌드는 통과하므로 이 테스트로 막는다.)
        var catalogDisplayTexts = new Easislides.Wpf.Input.CommandCatalog()
            .GetDefaultShortcuts()
            .Select(s => s.DisplayText)
            .ToHashSet();

        var gestures = System.Text.RegularExpressions.Regex
            .Matches(Xaml, "InputGestureText=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();

        gestures.Should().NotBeEmpty("메뉴에 단축키 힌트가 있어야 함");
        gestures.Should().OnlyContain(g => catalogDisplayTexts.Contains(g),
            "메뉴의 모든 단축키 힌트는 실제 카탈로그 단축키여야 한다(가짜 힌트 금지)");
    }

    private static string MenuItemOpeningFor(string commandOrClick)
    {
        var xaml = Xaml;
        var commandBinding = $"Command=\"{{Binding {commandOrClick}}}\"";
        var clickBinding = $"Click=\"{commandOrClick}\"";
        var marker = xaml.IndexOf(commandBinding, StringComparison.Ordinal);
        if (marker < 0)
        {
            marker = xaml.IndexOf(clickBinding, StringComparison.Ordinal);
        }

        marker.Should().BeGreaterThanOrEqualTo(0, $"{commandOrClick} 메뉴 항목이 있어야 함");

        var start = xaml.LastIndexOf("<MenuItem", marker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0, $"{commandOrClick} 는 MenuItem 에 배선돼야 함");

        var end = xaml.IndexOf(">", marker, StringComparison.Ordinal);
        end.Should().BeGreaterThan(marker, $"{commandOrClick} MenuItem 여는 태그를 찾을 수 있어야 함");

        return xaml[start..(end + 1)];
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
