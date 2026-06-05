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

    private static string AppCode => File.ReadAllText(
        Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "App.xaml.cs"),
        Encoding.UTF8);

    private static string TabViewXaml => File.ReadAllText(
        Path.Combine(FindRepositoryRoot(), "Easislides.Wpf", "Controls", "EsTabView.xaml"),
        Encoding.UTF8);

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string SectionBetween(string text, string start, string end)
    {
        var startIndex = text.IndexOf(start, StringComparison.Ordinal);
        startIndex.Should().BeGreaterThanOrEqualTo(0, $"section start '{start}' should exist");
        var endIndex = text.IndexOf(end, startIndex, StringComparison.Ordinal);
        endIndex.Should().BeGreaterThan(startIndex, $"section end '{end}' should exist after '{start}'");
        return text[startIndex..endIndex];
    }

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
    [InlineData("LiveNextShortcutCommand")]
    [InlineData("LivePreviousShortcutCommand")]
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
    public void BibleSourceLoad_RetriesWhenInitialLoadFindsNoVersions()
    {
        // FrmMain 1:1: 성경 데이터 로드가 실패하거나 0건이면 탭 재선택/추가 시 다시 시도해야 한다.
        // `_bibleLoadedOnce = true` 를 선반영하면 현장에서는 빈 성경 탭이 고정된다.
        CodeBehind.Should().Contain("private async Task EnsureBibleLoadedOnceAsync()");
        CodeBehind.Should().Contain("_bibleLoadedOnce = _viewModel.Bible.Versions.Count > 0;");
        CodeBehind.Should().NotContain("_bibleLoadedOnce = true;");
        CodeBehind.Should().Contain("await EnsureBibleLoadedOnceAsync().ConfigureAwait(true);");
    }

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
        code.Should().Contain("await EnsureBibleLoadedOnceAsync().ConfigureAwait(true);", "Bible data must be ready from the main shell startup");
        code.Should().Contain("private async Task EnsureBibleLoadedOnceAsync()", "Bible startup load should use the same retry-aware guard as tab selection");
    }

    [Fact]
    public void MainWindow_UsesFrmMainStyleSourcePreviewOutputPanes()
    {
        var xaml = Xaml;

        foreach (var pane in new[]
        {
            "ClassicFrmMainConsole",
            "ClassicSourcePane",
            "ClassicRightConsole",
            "ClassicPreviewPane",
            "ClassicPreviewSlidePane",
            "ClassicOutputPane",
            "ClassicOutputSlidePane",
        })
        {
            xaml.Should().Contain($"x:Name=\"{pane}\"", $"{pane} keeps the FrmMain source/preview/output geometry discoverable");
        }

        xaml.Should().Contain("Tag=\"splitContainerMain\"", "the outer WPF console should explicitly map to FrmMain splitContainerMain");
        xaml.Should().Contain("Tag=\"splitContainer2\"", "the right Preview/Output console should explicitly map to FrmMain splitContainer2");
        xaml.Should().Contain("Grid.Column=\"2\"", "Output should occupy the right-side column inside splitContainer2, not the old inspector slot");
        xaml.Should().Contain("Grid.Row=\"2\"", "Preview and Output should both have lower slide surfaces like FrmMain");
    }

    [Fact]
    public void ClassicFrmMainConsole_UsesNamedLegacyRegionColumnsAndRows()
    {
        var xaml = Xaml;

        xaml.Should().Contain("x:Name=\"ClassicSourceColumn\"", "left source/list column should be explicit for FrmMain 1:1 mapping");
        xaml.Should().Contain("x:Name=\"ClassicRightColumn\"", "right Preview/Output splitContainer2 column should be explicit");
        xaml.Should().Contain("Width=\"0.75*\"", "left column should scale like the legacy splitter instead of staying a narrow fixed rail");
        xaml.Should().Contain("MinWidth=\"320\"", "left column should keep the source browser usable at smaller sizes");
        xaml.Should().Contain("Tag=\"splitContainer1\"", "left source/list pane should keep the FrmMain splitContainer1 role");
        xaml.Should().Contain("Tag=\"tabControlSource\"", "upper-left source tabs should keep the FrmMain tabControlSource role");
        xaml.Should().Contain("Tag=\"tabControlLists\"", "lower-left list tabs should keep the FrmMain tabControlLists role");
        xaml.Should().Contain("Tag=\"splitContainerPreview.Panel1\"", "Preview top pane should map to splitContainerPreview.Panel1");
        xaml.Should().Contain("Tag=\"splitContainerPreview.Panel2\"", "Preview lower pane should map to splitContainerPreview.Panel2");
        xaml.Should().Contain("x:Name=\"ClassicPreviewColumn\"", "Preview column should be distinct from Output");
        xaml.Should().Contain("x:Name=\"ClassicOutputColumn\"", "Output column should be distinct from Preview");
        xaml.Should().Contain("x:Name=\"ClassicTopControlRow\"", "top text/thumbnail/control row should mirror FrmMain splitContainerPreview/Output Panel1");
        xaml.Should().Contain("x:Name=\"ClassicBottomScreenRow\"", "bottom screen row should mirror FrmMain splitContainerPreview/Output Panel2");
    }

    [Fact]
    public void ClassicOutputPane_ShowsPowerPointThumbnailsAndLargeOutputSlide()
    {
        var xaml = Xaml;

        xaml.Should().Contain("x:Name=\"ClassicOutputPane\"", "right-top Output pane should stay distinct");
        xaml.Should().Contain("Tag=\"splitContainerOutput.Panel1\"", "right-top Output pane should map to splitContainerOutput.Panel1");
        xaml.Should().Contain("Grid.Column=\"2\"", "Output panes stay in the right column inside splitContainer2");
        xaml.Should().Contain("Grid.Row=\"0\"", "right-top Output pane hosts thumbnails");
        xaml.Should().Contain("x:Name=\"flowLayoutOutputLyrics\"", "right-top Output pane should also expose FrmMain flowLayoutOutputLyrics for song/Bible output");
        xaml.Should().Contain("Tag=\"flowLayoutOutputLyrics\"", "the non-PPT Output lyrics role should remain explicit");
        xaml.Should().Contain("<ItemsControl x:Name=\"flowLayoutOutputLyrics\"",
            "FrmMain flowLayoutOutputLyrics should map to the clickable Output page-card surface, not only the outer scroller");
        xaml.Should().Contain("x:Name=\"ClassicOutputLyricsScrollViewer\"",
            "the Output page-card surface should keep a separate named scroller around the FrmMain flow layout");
        xaml.Should().Contain("ItemsSource=\"{Binding OutputLyricsPages}\"", "Output lyrics surface should render prepared/live Output pages as FrmMain-style flow items");
        xaml.Should().Contain("GoToOutputLyricsPageCommand", "Output lyrics page card clicks should navigate the Output item, not the selected Preview item");
        xaml.Should().Contain("Visibility=\"{Binding HasOutputLyricsText, Converter={StaticResource BoolToVis}}\"",
            "non-PPT Output lyrics should only show when the Output target has lyrics/body text");
        xaml.Should().Contain("x:Name=\"ClassicOutputThumbnailGrid\"", "right-top Output pane should match FrmMain PPT thumbnail mode");
        xaml.Should().Contain("Visibility=\"{Binding IsOutputPowerPointContext, Converter={StaticResource BoolToVis}}\"",
            "PPT thumbnails should show for the OutputItem context rather than every selected Preview item");
        xaml.Should().Contain("ItemsSource=\"{Binding OutputPowerPoint.Thumbnails}\"", "Output thumbnails must use the live/output PPT thumbnail source");
        xaml.Should().Contain("Command=\"{Binding DataContext.GoToOutputSlideCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}\"",
            "thumbnail clicks should navigate the output PPT slide, not the selected preview deck");
        var outputItemUp = SectionBetween(xaml, "x:Name=\"OutputBtnItemUp\"", "x:Name=\"OutputBtnItemDown\"");
        outputItemUp.Should().Contain("Command=\"{Binding PreviousOutputItemCommand}\"",
            "Output item-up button should target the live/prepared OutputItem, not the selected Preview item");
        outputItemUp.Should().NotContain("PreviousItemCommand");
        var outputItemDown = SectionBetween(xaml, "x:Name=\"OutputBtnItemDown\"", "x:Name=\"OutputBtnSlideUp\"");
        outputItemDown.Should().Contain("Command=\"{Binding NextOutputItemCommand}\"",
            "Output item-down button should target the live/prepared OutputItem, not the selected Preview item");
        outputItemDown.Should().NotContain("NextItemCommand");
        xaml.Should().Contain("Command=\"{Binding PreviousOutputSlideCommand}\"", "Output slide-up button should target the live output deck");
        xaml.Should().Contain("Command=\"{Binding NextOutputSlideCommand}\"", "Output slide-down button should target the live output deck");
        xaml.Should().Contain("x:Name=\"ClassicOutputSlidePane\"", "right-bottom Output pane should stay distinct");
        xaml.Should().Contain("Tag=\"splitContainerOutput.Panel2\"", "right-bottom Output pane should map to splitContainerOutput.Panel2");
        xaml.Should().Contain("x:Name=\"ClassicOutputLargeSlideImage\"", "right-bottom Output pane should expose the large slide surface");
        xaml.Should().Contain("Source=\"{Binding OutputVisualSource}\"", "large Output slide should show the live/output PPT or image visual when available");
        xaml.Should().Contain("Visibility=\"{Binding HasOutputVisualSource, Converter={StaticResource BoolToVis}}\"",
            "the large visual layer should only cover the Output text frame when an actual visual source exists");
    }

    [Fact]
    public void ClassicOutputTopPane_DoesNotAddModernStatusFooterBelowFrmMainFlowSurfaces()
    {
        var xaml = Xaml;

        var outputTopPane = SectionBetween(
            xaml,
            "x:Name=\"ClassicOutputInfo\"",
            "x:Name=\"ClassicOutputSlidePane\"");

        outputTopPane.Should().Contain("x:Name=\"flowLayoutOutputLyrics\"",
            "Output top pane should keep the FrmMain lyrics flow surface");
        outputTopPane.Should().Contain("x:Name=\"ClassicOutputPowerPointSurface\"",
            "Output top pane should keep the FrmMain PowerPoint flow surface");
        outputTopPane.Should().NotContain("LiveBar.StateLabel",
            "the right-top Output pane should not add a modern status footer below the FrmMain flow surfaces");
        outputTopPane.Should().NotContain("LiveBar.OutputMonitorName",
            "monitor status belongs to the operator/status surfaces, not an extra row under flowLayoutOutputPowerPoint");
        outputTopPane.Should().NotContain("OutputNavigationPositionLabel, StringFormat",
            "position is already exposed in OutputPanelDisplayName and the lower Output frame");
    }

    [Fact]
    public void ClassicPreviewAndOutputLargeScreens_KeepFrmMainSixteenNineFrames()
    {
        var xaml = Xaml;

        var preview = SectionBetween(xaml, "x:Name=\"ClassicPreviewHolder\"", "</Viewbox>");
        preview.Should().Contain("Tag=\"PreviewHolder\"", "Preview lower pane should keep the FrmMain PreviewHolder role");
        preview.Should().Contain("Stretch=\"Uniform\"", "Preview screen should scale as one slide frame instead of stretching to pane shape");
        preview.Should().Contain("Width=\"960\"", "Preview screen should use a fixed 16:9 design surface");
        preview.Should().Contain("Height=\"540\"", "Preview screen should use a fixed 16:9 design surface");
        preview.Should().Contain("x:Name=\"ClassicPreviewBack\"", "Preview background should stay inside the fixed frame like FrmMain PreviewBack");
        preview.Should().Contain("x:Name=\"ClassicPreviewLargeLyricsText\"", "non-PPT Preview lyrics should render in the lower preview frame");
        preview.Should().Contain("Text=\"{Binding PreviewLyricsText}\"", "the lower Preview frame should follow the selected Preview page text");
        preview.Should().Contain("Text=\"{Binding PreviewNavigationPositionLabel}\"",
            "the lower Preview frame should show the selected Preview page/slide position");
        preview.Should().Contain("Visibility=\"{Binding HasPreviewLyricsText, Converter={StaticResource BoolToVis}}\"",
            "the lower Preview lyrics layer should only appear for selected text items");
        preview.Should().Contain("Visibility=\"{Binding HasPreviewVisualSource, Converter={StaticResource BoolToVis}}\"",
            "image/PPT PreviewSource should cover the text frame only when an actual visual source exists");
        preview.Should().Contain("Source=\"{Binding PreviewVisualSource}\"",
            "the large Preview frame should use the rendered PPT/image source rather than only the raw selected item source");

        var output = SectionBetween(xaml, "x:Name=\"ClassicOutputHolder\"", "</Viewbox>");
        output.Should().Contain("Tag=\"OutputHolder\"", "Output lower pane should keep the FrmMain OutputHolder role");
        output.Should().Contain("Stretch=\"Uniform\"", "Output screen should scale as one slide frame instead of stretching to pane shape");
        output.Should().Contain("Width=\"960\"", "Output screen should use the same 16:9 frame as the live output slide");
        output.Should().Contain("Height=\"540\"", "Output screen should use the same 16:9 frame as the live output slide");
        output.Should().Contain("x:Name=\"ClassicOutputBack\"", "the output background stays inside the fixed frame");
        output.Should().Contain("x:Name=\"ClassicOutputLargeLyricsText\"", "non-PPT Output lyrics should render in the lower live preview frame");
        output.Should().Contain("Text=\"{Binding OutputLyricsText}\"", "the lower Output frame should follow OutputItem/live lyrics, not selected Preview lyrics");
        output.Should().Contain("Text=\"{Binding OutputNavigationPositionLabel}\"",
            "the lower Output frame should show prepared/live Output page or slide position");
        output.Should().Contain("Visibility=\"{Binding HasOutputLyricsText, Converter={StaticResource BoolToVis}}\"",
            "the lower Output lyrics layer should only appear for prepared/live text items");
        output.Should().Contain("x:Name=\"ClassicOutputLargeSlideImage\"", "the visual overlay stays inside the fixed frame");
        output.Should().Contain("Source=\"{Binding OutputVisualSource}\"",
            "the lower Output frame should use the prepared/live visual source rather than only the PPT VM");
        output.Should().Contain("FillMode=\"{Binding OutputVisualFillMode}\"",
            "image Output items should keep their FrmMain-style fit/fill mode in the large frame");
        output.Should().Contain("Visibility=\"{Binding HasOutputVisualSource, Converter={StaticResource BoolToVis}}\"",
            "the lower Output visual layer should only appear for prepared/live visual items");
    }

    [Fact]
    public void ClassicPreviewAndOutputPowerPointThumbnails_MapFocusedKeyboardNavigationIndependently()
    {
        var code = CodeBehind;

        code.Should().Contain("TryHandlePreviewOutputPowerPointKey(e)",
            "focused PPT thumbnail keyboard handling should run before global Space/F shortcuts");
        code.Should().Contain("ClassicPreviewPowerPointThumbnailGrid.IsKeyboardFocusWithin",
            "Preview PPT thumbnail focus should be detected separately");
        code.Should().Contain("ClassicOutputThumbnailGrid.IsKeyboardFocusWithin",
            "Output PPT thumbnail focus should be detected separately");
        code.Should().Contain("_viewModel.PreviousSlideCommand.Execute(null)",
            "Preview Up should move only the selected preview deck");
        code.Should().Contain("_viewModel.NextSlideCommand.Execute(null)",
            "Preview Down/Space should move only the selected preview deck");
        code.Should().Contain("_viewModel.GoToSlideCommand.Execute(slideNumber)",
            "Preview Left/Right should jump inside the preview deck");
        code.Should().Contain("ExecuteCommand(_viewModel.PreviousPreviewItemCommand)",
            "Preview PageUp should move the selected Preview item like FrmMain");
        code.Should().Contain("ExecuteCommand(_viewModel.NextPreviewItemCommand)",
            "Preview PageDown/Tab should move the selected Preview item like FrmMain");
        code.Should().Contain("ExecuteCommand(_viewModel.FirstPreviewItemCommand)",
            "Preview Home should move to the first Preview item like FrmMain");
        code.Should().Contain("ExecuteCommand(_viewModel.LastPreviewItemCommand)",
            "Preview End should move to the last Preview item like FrmMain");
        code.Should().Contain("_viewModel.PreviousOutputSlideCommand.Execute(null)",
            "Output Up should move only the live output deck");
        code.Should().Contain("_viewModel.NextOutputSlideCommand.Execute(null)",
            "Output Down/Space should move only the live output deck");
        code.Should().Contain("_viewModel.GoToOutputSlideCommand.Execute(slideNumber)",
            "Output Left/Right should jump inside the live output deck");
        code.Should().Contain("ExecuteCommand(_viewModel.PreviousOutputItemCommand)",
            "Output PageUp should move the live/prepared Output item like FrmMain");
        code.Should().Contain("ExecuteCommand(_viewModel.NextOutputItemCommand)",
            "Output PageDown/Tab should move the live/prepared Output item like FrmMain");
        CountOccurrences(code, "case Key.Tab:").Should().BeGreaterThanOrEqualTo(2,
            "FrmMain PPT thumbnail focus treats Tab as NextOne for both Preview and Output panels");
        code.Should().Contain("ExecuteCommand(_viewModel.FirstOutputItemCommand)",
            "Output Home should move to the first live/prepared Output item like FrmMain");
        code.Should().Contain("ExecuteCommand(_viewModel.LastOutputItemCommand)",
            "Output End should move to the last live/prepared Output item like FrmMain");
    }

    [Fact]
    public void ClassicPreviewPowerPointTopPane_IsThumbnailsOnlyLikeFrmMainFlowLayout()
    {
        var xaml = Xaml;

        var previewPowerPointPane = SectionBetween(
            xaml,
            "x:Name=\"ClassicPreviewPowerPointSurface\"",
            "AutomationProperties.Name=\"Media\"");

        previewPowerPointPane.Should().Contain("Tag=\"flowLayoutPreviewPowerPoint\"",
            "the Preview PowerPoint top pane should keep the legacy flowLayoutPreviewPowerPoint role");
        previewPowerPointPane.Should().NotContain("PreviousSlideCommand",
            "Preview slide navigation belongs to the lower FrmMain strip buttons, not a modern duplicate panel");
        previewPowerPointPane.Should().NotContain("NextSlideCommand",
            "Preview slide navigation belongs to the lower FrmMain strip buttons, not a modern duplicate panel");
        previewPowerPointPane.Should().NotContain("PowerPoint.StatusText",
            "FrmMain flowLayoutPreviewPowerPoint is a thumbnail surface, not a separate status panel");
        previewPowerPointPane.Should().NotContain("PowerPointFileCount",
            "FrmMain flowLayoutPreviewPowerPoint is a thumbnail surface, not a separate deck-count panel");
    }

    [Fact]
    public void ClassicPreviewAndOutputLyricsSurfaces_MapFocusedKeyboardNavigationIndependently()
    {
        var xaml = Xaml;
        var code = CodeBehind;

        foreach (var name in new[]
        {
            "ClassicPreviewInfo",
            "ClassicPreviewSlidePane",
            "ClassicPreviewHolder",
            "ClassicOutputInfo",
            "flowLayoutOutputLyrics",
            "ClassicOutputSlidePane",
            "ClassicOutputHolder",
            "ClassicOutputBack",
        })
        {
            xaml.Should().Contain($"x:Name=\"{name}\"", $"{name} should remain a focusable FrmMain keyboard surface");
        }

        xaml.Should().Contain("MouseDown=\"ClassicKeyboardSurface_MouseDown\"",
            "clicking a Preview/Output lyrics surface should give it keyboard focus like FrmMain flow panels");

        var powerPointIndex = code.IndexOf("TryHandlePreviewOutputPowerPointKey(e)", StringComparison.Ordinal);
        var lyricsIndex = code.IndexOf("TryHandleFocusedPreviewOutputLyricsKey(e)", StringComparison.Ordinal);
        var globalVerseIndex = code.IndexOf("TryHandleVerseJumpKey(e)", StringComparison.Ordinal);

        lyricsIndex.Should().BeGreaterThan(powerPointIndex,
            "focused lyrics routing should run after the more-specific PPT thumbnail routing");
        lyricsIndex.Should().BeLessThan(globalVerseIndex,
            "focused Output verse keys must not fall through to the global Preview verse jump");

        code.Should().Contain("ClassicPreviewInfo.IsKeyboardFocusWithin",
            "PreviewInfo focus should route keys to the selected Preview item");
        code.Should().Contain("ClassicOutputInfo.IsKeyboardFocusWithin",
            "OutputInfo focus should route keys to the live Output item");
        code.Should().Contain("flowLayoutOutputLyrics.IsKeyboardFocusWithin",
            "Output lyrics flow focus should route keys to the live Output item");
        code.Should().Contain("_viewModel.JumpToLyricsSectionCommand",
            "Preview verse keys should keep targeting the selected Preview item");
        code.Should().Contain("_viewModel.JumpToOutputLyricsSectionCommand",
            "Output verse keys should target the live Output item independently");
        code.Should().Contain("var modifiers = Keyboard.Modifiers;",
            "focused key routing should capture the current modifier state before dispatch");
        code.Should().Contain("AllowsFrmMainVerseKeyModifiers(modifiers)",
            "focused FrmMain Preview/Output surfaces should allow Shift verse shortcuts while still blocking Ctrl/Alt/Win");
        code.Should().Contain("TryExecutePreviewLyricsKey(e.Key, modifiers)",
            "Preview lyrics focus should pass Shift state into legacy verse shortcut mapping");
        code.Should().Contain("TryExecuteOutputLyricsKey(e.Key, modifiers)",
            "Output lyrics focus should pass Shift state into legacy verse shortcut mapping");
        code.Should().Contain("TryExecutePreviewPowerPointKey(e.Key, modifiers)",
            "Preview PPT thumbnail focus should preserve FrmMain shifted verse shortcut handling");
        code.Should().Contain("TryExecuteOutputPowerPointKey(e.Key, modifiers)",
            "Output PPT thumbnail focus should preserve FrmMain shifted verse shortcut handling");
        code.Should().Contain("VerseJumpKeyMap.MapKeyToLabel(key, modifiers)",
            "legacy shortcut mapping should distinguish Shift+B/P/W/Q/T from bare letter keys");
        code.Should().Contain("_viewModel.PreviousLyricsPageCommand",
            "Preview Up/PageUp should use the selected item's lyrics-page command");
        code.Should().Contain("_viewModel.NextLyricsPageCommand",
            "Preview Down/PageDown/Space should use the selected item's lyrics-page command");
        code.Should().Contain("_viewModel.PreviousOutputSlideCommand",
            "Output Up/PageUp should use the live Output navigation command");
        code.Should().Contain("_viewModel.NextOutputSlideCommand",
            "Output Down/PageDown/Space should use the live Output navigation command");

        var previewLyricsRouting = SectionBetween(
            code,
            "private bool TryExecutePreviewLyricsKey",
            "private bool TryExecuteOutputLyricsKey");
        previewLyricsRouting.Should().Contain("ExecuteCommand(_viewModel.FirstPreviewItemCommand)",
            "Preview lyrics focus Home should move the selected Preview context to the first item like FrmMain focused panes");
        previewLyricsRouting.Should().Contain("ExecuteCommand(_viewModel.LastPreviewItemCommand)",
            "Preview lyrics focus End should move the selected Preview context to the last item like FrmMain focused panes");

        var outputLyricsRouting = SectionBetween(
            code,
            "private bool TryExecuteOutputLyricsKey",
            "private static bool TryExecuteVerseJumpKey");
        outputLyricsRouting.Should().Contain("ExecuteCommand(_viewModel.FirstOutputItemCommand)",
            "Output lyrics focus Home should move the live/prepared Output context to the first item, not the selected Preview item");
        outputLyricsRouting.Should().Contain("ExecuteCommand(_viewModel.LastOutputItemCommand)",
            "Output lyrics focus End should move the live/prepared Output context to the last item, not the selected Preview item");
    }

    [Fact]
    public void LeftBrowserTabs_MatchFrmMainSourceRolesAndOrder()
    {
        var xaml = Xaml;
        xaml.Should().Contain("x:Name=\"LeftBrowserTabs\"", "left source browser stays first-screen");
        xaml.Should().Contain("TabStripPlacement=\"Bottom\"", "FrmMain source tabs are bottom-aligned");
        xaml.Should().Contain("Style=\"{StaticResource EsTabView.FrmMainBottom}\"",
            "LeftBrowserTabs must use a compact bottom tab template instead of the large WPF UI tab header layout");

        var tabs = TabViewXaml;
        tabs.Should().Contain("x:Key=\"EsTabView.FrmMainBottom\"", "FrmMain source/list tabs need their own compact bottom style");
        tabs.Should().Contain("x:Key=\"EsTabView.FrmMainBottomItem\"", "FrmMain tabs need compact item chrome");
        tabs.Should().Contain("PART_SelectedContentHost", "the FrmMain tab template should render selected content separately from the bottom tab strip");
        tabs.Should().Contain("Grid.Row=\"1\"", "the FrmMain tab template should dock the header strip below selected content");

        var cursor = -1;
        foreach (var tag in new[]
        {
            "Folders",
            "InfoScreenSource",
            "PowerPointSource",
            "Bibles",
            "ImagesSource",
            "MediaSource",
            "DefaultSource",
        })
        {
            var index = xaml.IndexOf($"Tag=\"{tag}\"", StringComparison.Ordinal);
            index.Should().BeGreaterThan(cursor, $"{tag} should appear in FrmMain source-tab order");
            cursor = index;
        }
    }

    [Fact]
    public void LeftListTabs_UseSameFrmMainCompactBottomTabTemplate()
    {
        var xaml = Xaml;
        xaml.Should().Contain("x:Name=\"LeftListTabs\"", "lower-left Worship/Praise tabs stay first-screen");
        xaml.Should().Contain("Tag=\"tabControlLists\"", "lower-left tabs should keep the FrmMain tabControlLists role");

        var leftListTabsStart = xaml.IndexOf("x:Name=\"LeftListTabs\"", StringComparison.Ordinal);
        leftListTabsStart.Should().BeGreaterThanOrEqualTo(0);
        var leftListTabsEnd = xaml.IndexOf("<TabItem Tag=\"WorshipList\"", leftListTabsStart, StringComparison.Ordinal);
        leftListTabsEnd.Should().BeGreaterThan(leftListTabsStart);
        var leftListTabsDeclaration = xaml[leftListTabsStart..leftListTabsEnd];

        leftListTabsDeclaration.Should().Contain("TabStripPlacement=\"Bottom\"",
            "FrmMain lower-left tabs are bottom-aligned");
        leftListTabsDeclaration.Should().Contain("Style=\"{StaticResource EsTabView.FrmMainBottom}\"",
            "the Worship List/Praise Book tabs should not render as large modern header buttons");
    }

    [Fact]
    public void FoldersTab_UsesFrmMainSongFolderToolStripAndSongsListRoles()
    {
        var xaml = Xaml;

        xaml.Should().Contain("x:Name=\"ClassicFoldersSourceGrid\"", "Folders source should be a named legacy mapping surface");
        xaml.Should().Contain("x:Name=\"ClassicSongFolderCombo\"", "SongFolder combo should stay visible at the top of Folders");
        xaml.Should().Contain("Tag=\"SongFolder\"", "SongFolder role should remain explicit without hurting the accessible label");
        xaml.Should().Contain("x:Name=\"ClassicFoldersToolStrip\"", "Folders toolbar should stay attached to SongFolder like FrmMain");
        xaml.Should().Contain("Tag=\"Folders_WordCount\"", "Folders_WordCount sorting role should be present on the first-screen toolbar");
        xaml.Should().Contain("<ListView Grid.Row=\"3\"", "SongsList should render as a compact details-style ListView");
        xaml.Should().Contain("x:Name=\"LibrarySongList\"", "existing drag and double-click handlers should keep their stable target");
        xaml.Should().Contain("Tag=\"SongsList\"", "LibrarySongList should be explicitly mapped to FrmMain SongsList");
        xaml.Should().Contain("SelectionMode=\"Extended\"", "FrmMain SongsList supports multi-select add/context gestures");
        xaml.Should().Contain("<GridView AllowsColumnReorder=\"False\">", "legacy ListView Details behavior should hide headers but keep columns");
        xaml.Should().Contain("PreviewMouseMove=\"LibrarySongList_PreviewMouseMove\"", "song rows should keep drag-to-Worship behavior");
        xaml.Should().Contain("PreviewMouseRightButtonDown=\"LibrarySongList_PreviewMouseRightButtonDown\"",
            "right-clicking a song row should select it before opening CMenuSongs");
        xaml.Should().Contain("KeyDown=\"LibrarySongList_KeyDown\"",
            "SongsList should keep FrmMain's Delete-key route separate from the generic Enter-add source handler");
        xaml.Should().Contain("MouseAction=\"LeftDoubleClick\"", "song rows should keep double-click add behavior");
    }

    [Fact]
    public void FoldersTab_ExposesFrmMainSongsContextMenu()
    {
        var xaml = Xaml;
        var code = CodeBehind;

        xaml.Should().Contain("x:Name=\"CMenuSongs\"", "Folders SongsList should expose the legacy song context menu");
        xaml.Should().Contain("Opened=\"CMenuSongs_Opened\"", "CMenuSongs should compute item enablement at open time like FrmMain");
        xaml.Should().Contain("x:Name=\"CMenuSongs_SelectAll\"");
        xaml.Should().Contain("x:Name=\"CMenuSongs_UnselectAll\"");
        xaml.Should().Contain("x:Name=\"CMenuSongs_AddShow\"");
        xaml.Should().Contain("x:Name=\"CMenuSongs_Edit\"");
        xaml.Should().Contain("x:Name=\"CMenuSongs_Copy\"");
        xaml.Should().Contain("x:Name=\"CMenuSongs_Delete\"");
        xaml.Should().Contain("x:Name=\"CMenuSongs_Refresh\"");

        code.Should().Contain("private void LibrarySongList_PreviewMouseRightButtonDown",
            "right-click commands should target the song row under the pointer");
        code.Should().Contain("private async void CMenuSongs_AddShow_Click",
            "Add && Show should route from the FrmMain song context menu");
        code.Should().Contain("viewModel.SelectedItem = added[0]",
            "FrmMain CMenuSongs_AddShow sends the first newly added Worship List row live");
        code.Should().Contain("await viewModel.PreviewToLiveCommand.ExecuteAsync(null).ConfigureAwait(true)",
            "Add && Show should publish through the same Preview-to-Live path as the visible FrmMain LIVE button");
        code.Should().Contain("private async Task OpenSelectedLibrarySongEditorAsync",
            "CMenuSongs_Edit should open the selected library song editor, not the Worship List item editor");
        code.Should().Contain("private void CMenuSongs_Delete_Click",
            "CMenuSongs_Delete should reuse the main song delete launcher with the selected source song context");
        code.Should().Contain("SongDelete_Click(sender, e)",
            "Delete menu and Delete key should share the existing song delete dialog path");
    }

    [Fact]
    public void SourceLists_MapPlainEnterToFrmMainAddGesture()
    {
        var xaml = Xaml;
        var code = CodeBehind;

        CountOccurrences(xaml, "KeyDown=\"SourceListAddOnEnter_KeyDown\"")
            .Should().BeGreaterThanOrEqualTo(6, "Folders, InfoScr, PowerPoint, Media, and search result lists should add with Enter");
        xaml.Should().Contain("PreviewKeyDown=\"BiblePassageBox_PreviewKeyDown\"",
            "selected Bible text should add with Enter without waiting for a context menu");
        xaml.Should().Contain("KeyDown=\"PraiseBookItems_KeyDown\"",
            "lower-left PraiseBookItems should add the focused selection with Enter");

        code.Should().Contain("private async void SourceListAddOnEnter_KeyDown",
            "source-list Enter should be handled in MainWindow where the active source tab is known");
        code.Should().Contain("private void LibrarySongList_KeyDown",
            "Folders SongsList needs a dedicated Delete-key path in addition to Enter add");
        code.Should().Contain("await AddSelectedSourceToWorshipListAsync(viewModel).ConfigureAwait(true)",
            "Enter should reuse the same active-source router as WL_Add");
        code.Should().Contain("private async void BiblePassageBox_PreviewKeyDown",
            "Bible passage Enter should be handled before the read-only TextBox consumes it");
        code.Should().Contain("private async void PraiseBookItems_KeyDown",
            "PraiseBook Enter should be explicitly mapped");
        code.Should().Contain("AddSelectedPraiseBookEntryToWorshipList",
            "PraiseBook Enter and double-click should share the same add path");
    }

    [Fact]
    public void BiblesTab_ExposesFrmMainBibleContextMenu()
    {
        var xaml = Xaml;
        var code = CodeBehind;

        xaml.Should().Contain("Tag=\"Bibles\"", "FrmMain keeps Bibles as a source tab");
        xaml.Should().Contain("x:Name=\"BookLookup\"", "Bibles tab should expose the FrmMain book selector role");
        xaml.Should().Contain("Tag=\"BookLookup\"", "BookLookup role should be explicit for 1:1 mapping");
        xaml.Should().Contain("SelectedItem=\"{Binding Bible.SelectedBook, Mode=TwoWay}\"",
            "changing BookLookup should move the selected Bible book, not require a detached modal flow");
        xaml.Should().Contain("x:Name=\"BibleUserLookup\"", "Bibles tab should expose the FrmMain typed-reference textbox role");
        xaml.Should().Contain("Tag=\"BibleUserLookup\"", "typed reference input should keep the legacy BibleUserLookup role");
        xaml.Should().Contain("KeyDown=\"BibleReferenceBox_KeyDown\"", "BibleUserLookup Enter should submit like FrmMain");
        xaml.Should().Contain("x:Name=\"Bibles_Go\"", "Bibles tab should expose the FrmMain Go toolstrip button role");
        xaml.Should().Contain("Tag=\"Bibles_Go\"", "Go button should keep the legacy Bibles_Go role");
        xaml.Should().Contain("Click=\"JumpBibleReference_Click\"", "Bibles_Go should submit the typed reference");
        xaml.Should().Contain("x:Name=\"Bibles_ShowVerses\"", "Bibles tab should expose the FrmMain show-verses toggle role");
        xaml.Should().Contain("Tag=\"Bibles_ShowVerses\"", "Show-verses toggle should keep the legacy role");
        xaml.Should().Contain("IsChecked=\"{Binding Bible.ShowVerses, Mode=TwoWay}\"",
            "Bibles_ShowVerses should drive the same VM state used by passage loading");
        xaml.Should().Contain("x:Name=\"TabBibleVersions\"", "Bibles tab should expose the FrmMain version-tab role");
        xaml.Should().Contain("Tag=\"TabBibleVersions\"", "version selector should keep the legacy TabBibleVersions role");
        xaml.Should().Contain("ItemsSource=\"{Binding Bible.Versions}\"",
            "TabBibleVersions should be backed by the loaded legacy Bible versions");
        xaml.Should().Contain("SelectedItem=\"{Binding Bible.SelectedVersion, Mode=TwoWay}\"",
            "changing TabBibleVersions should change the active Bible version");
        xaml.Should().Contain("Tag=\"BibleText\"", "Bible text surface should keep the FrmMain BibleText role");
        xaml.Should().Contain("x:Name=\"BiblePassageBox\"", "Bible text should keep a stable selection surface");
        xaml.Should().Contain("x:Name=\"CMenuBible\"", "Bible text context menu should map to FrmMain CMenuBible");
        xaml.Should().Contain("Opened=\"BibleContextMenu_Opened\"", "menu enablement should follow FrmMain's opening-time rules");
        xaml.Should().Contain("x:Name=\"CMenuBible_SelectAll\"", "Bible menu should expose Select All");
        xaml.Should().Contain("x:Name=\"CMenuBible_UnselectAll\"", "Bible menu should expose Unselect All");
        xaml.Should().Contain("x:Name=\"CMenuBible_AddShow\"", "Bible menu should expose Add && Show");
        xaml.Should().Contain("x:Name=\"CMenuBible_AddRegion2\"", "Bible menu should expose Add Region 2");
        xaml.Should().Contain("ItemsSource=\"{Binding PlacementTarget.DataContext.Bible.Region2VersionOptions, RelativeSource={RelativeSource AncestorType=ContextMenu}}\"",
            "Add Region 2 should build its submenu from the loaded Bible versions");
        xaml.Should().Contain("x:Name=\"CMenuBible_Copy\"", "Bible menu should expose Copy");
        xaml.Should().Contain("x:Name=\"CMenuBible_CopyInfoScreen\"", "Bible menu should expose Copy to InfoScreen");

        code.Should().Contain("private void BibleContextMenu_Opened", "Bible menu should compute enabled states when opened");
        code.Should().Contain("CMenuBible_AddRegion2.IsEnabled = hasSelection", "Region 2 should require a selected passage");
        code.Should().Contain("CMenuBible_Copy.IsEnabled = hasSelection", "Copy should require selected passage text");
        code.Should().Contain("UnselectAllBiblePassage_Click", "Unselect All should clear the Bible text selection");
    }

    [Fact]
    public void LeftBrowserTabs_ExposeInlinePowerPointAndMediaSources()
    {
        var xaml = Xaml;

        xaml.Should().Contain("Tag=\"InfoScreenSource\"", "FrmMain keeps InfoScr as a main-console source tab");
        xaml.Should().Contain("x:Name=\"InfoScreenSourceTab\"", "inline InfoScr tab needs a stable DataContext target");
        xaml.Should().Contain("x:Name=\"InlineInfoScreenList\"", "saved InfoScreens must be visible without opening the editor modal");
        xaml.Should().Contain("ItemsSource=\"{Binding Screens}\"", "inline InfoScr list reuses InfoScreenSourceViewModel");
        xaml.Should().Contain("SelectionMode=\"Extended\"", "InfoScreenList should preserve FrmMain multi-select behavior");
        xaml.Should().Contain("MouseDoubleClick=\"InlineInfoScreenList_MouseDoubleClick\"", "double-click should add selected InfoScreen");
        xaml.Should().Contain("PreviewMouseRightButtonDown=\"InlineInfoScreenList_PreviewMouseRightButtonDown\"",
            "right-clicking an InfoScreen row should select it before opening CMenuFiles");
        xaml.Should().Contain("PreviewMouseMove=\"InlineInfoScreenList_PreviewMouseMove\"", "InfoScreen rows should drag into the Worship List");
        xaml.Should().Contain("x:Name=\"CMenuInfoScreenFiles\"", "InfoScreenList should expose the FrmMain shared file context menu role");
        xaml.Should().Contain("Tag=\"CMenuFiles_SelectAll\"", "InfoScreen CMenuFiles should expose Select All with the legacy role");
        xaml.Should().Contain("x:Name=\"CMenuInfoScreenFiles_AddShow\"", "InfoScreen CMenuFiles should expose Add && Show");
        xaml.Should().Contain("x:Name=\"CMenuInfoScreenFiles_Refresh\"", "InfoScreen CMenuFiles should expose Refresh");

        xaml.Should().Contain("Tag=\"PowerPointSource\"", "FrmMain keeps PowerPoint as a main-console source tab");
        xaml.Should().Contain("x:Name=\"PowerPointSourceTab\"", "inline PowerPoint tab needs a stable DataContext target");
        xaml.Should().Contain("x:Name=\"PowerpointFolder\"", "PowerPoint source should expose the FrmMain PowerpointFolder combo role");
        xaml.Should().Contain("ItemsSource=\"{Binding FolderGroups}\"", "PowerpointFolder should list the root and subfolder groups");
        xaml.Should().Contain("SelectedItem=\"{Binding SelectedFolder, Mode=TwoWay}\"", "selecting a PowerpointFolder should reload that source folder");
        xaml.Should().Contain("x:Name=\"InlinePowerPointList\"", "PowerPoint files must be visible without opening a modal window");
        xaml.Should().Contain("Tag=\"PowerpointList\"", "PowerpointList should keep the FrmMain list role");
        xaml.Should().Contain("ItemsSource=\"{Binding Presentations}\"", "inline PowerPoint list reuses PowerPointLibraryViewModel");
        xaml.Should().Contain("PreviewMouseRightButtonDown=\"InlinePowerPointList_PreviewMouseRightButtonDown\"",
            "right-clicking a PowerPoint row should select it before opening CMenuFiles");
        xaml.Should().Contain("MouseDoubleClick=\"InlinePowerPointList_MouseDoubleClick\"", "double-click should add selected PowerPoint");
        xaml.Should().Contain("PreviewMouseMove=\"InlinePowerPointList_PreviewMouseMove\"", "PowerPoint rows should drag into the Worship List");
        xaml.Should().Contain("x:Name=\"CMenuPowerPointFiles\"", "PowerpointList should expose the FrmMain shared file context menu role");
        xaml.Should().Contain("x:Name=\"CMenuPowerPointFiles_AddShow\"", "PowerPoint CMenuFiles should expose Add && Show");
        xaml.Should().Contain("x:Name=\"CMenuPowerPointFiles_Refresh\"", "PowerPoint CMenuFiles should expose Refresh");
        xaml.Should().Contain("x:Name=\"PP_ListType\"", "PowerPoint source needs the FrmMain PP_ListType dropdown role");
        xaml.Should().Contain("x:Name=\"PP_ListStyle\"", "PP_ListType should expose FrmMain list style");
        xaml.Should().Contain("x:Name=\"PP_PreviewStyle\"", "PP_ListType should expose FrmMain preview style");
        xaml.Should().Contain("UseListStyleCommand", "PP_ListStyle should switch back to list mode");
        xaml.Should().Contain("UsePreviewStyleCommand", "PP_PreviewStyle should switch to thumbnail preview mode");
        xaml.Should().Contain("x:Name=\"flowLayoutExternalPowerPoint\"", "preview mode should expose the FrmMain thumbnail-flow role");
        xaml.Should().Contain("Tag=\"flowLayoutExternalPowerPoint\"", "thumbnail flow role should remain explicit");
        xaml.Should().Contain("x:Name=\"CMenuPowerPointPreviewFiles\"", "PowerPoint preview flow should expose the same CMenuFiles actions");
        xaml.Should().Contain("<WrapPanel Orientation=\"Horizontal\" />", "preview mode should use a flowing thumbnail layout");
        xaml.Should().Contain("Source=\"{Binding ThumbnailImage}\"", "PowerPoint preview style should render real first-slide thumbnails when available");
        xaml.Should().Contain("ToolTip=\"{Binding ThumbnailStatus}\"", "PowerPoint thumbnail cards should expose render status without replacing the file name");
        xaml.Should().Contain("IsPreviewStyle", "PowerpointList and flowLayoutExternalPowerPoint should toggle from the same listing style state");

        xaml.Should().Contain("Tag=\"MediaSource\"", "FrmMain keeps Media as a main-console source tab");
        xaml.Should().Contain("x:Name=\"MediaSourceTab\"", "inline Media tab needs a stable DataContext target");
        xaml.Should().Contain("x:Name=\"InlineMediaList\"", "Media files must be visible without opening a modal window");
        xaml.Should().Contain("ItemsSource=\"{Binding MediaFiles}\"", "inline Media list reuses MediaLibraryViewModel");
        xaml.Should().Contain("SelectionMode=\"Extended\"", "MediaList should preserve FrmMain multi-select behavior");
        xaml.Should().Contain("MouseDoubleClick=\"InlineMediaList_MouseDoubleClick\"", "double-click should add selected Media");
        xaml.Should().Contain("PreviewMouseRightButtonDown=\"InlineMediaList_PreviewMouseRightButtonDown\"",
            "right-clicking a Media row should select it before opening CMenuFiles");
        xaml.Should().Contain("PreviewMouseMove=\"InlineMediaList_PreviewMouseMove\"", "Media rows should drag into the Worship List");
        xaml.Should().Contain("x:Name=\"CMenuFiles\"", "MediaList should expose the FrmMain shared file context menu role");
        xaml.Should().Contain("x:Name=\"CMenuFiles_SelectAll\"", "CMenuFiles should expose Select All");
        xaml.Should().Contain("x:Name=\"CMenuFiles_UnselectAll\"", "CMenuFiles should expose Unselect All");
        xaml.Should().Contain("x:Name=\"CMenuFiles_AddShow\"", "CMenuFiles should expose Add && Show");
        xaml.Should().Contain("x:Name=\"CMenuFiles_Edit\"", "CMenuFiles should keep the legacy Edit item");
        xaml.Should().Contain("x:Name=\"CMenuFiles_Copy\"", "CMenuFiles should keep the legacy Copy item");
        xaml.Should().Contain("x:Name=\"CMenuFiles_Refresh\"", "CMenuFiles should expose Refresh");

        xaml.Should().Contain("Tag=\"ImagesSource\"", "FrmMain keeps Images as a main-console source tab");
        xaml.Should().Contain("x:Name=\"ImagesSourceTab\"", "inline Images tab needs a stable DataContext target");
        xaml.Should().Contain("x:Name=\"InlineImagesList\"", "Images must be visible without opening a modal window");
        xaml.Should().Contain("ItemsSource=\"{Binding Images}\"", "inline Images list reuses ImageLibraryViewModel");
        xaml.Should().Contain("Tag=\"flowLayoutImages\"", "inline Images list should keep the FrmMain flowLayoutImages role");
        xaml.Should().Contain("KeyDown=\"SourceListAddOnEnter_KeyDown\"", "Images Enter should reuse the left-source add/apply gesture");
        xaml.Should().Contain("ApplySelectedImageCommand", "inline Images should use FrmMain item-first/default-fallback apply semantics");
        xaml.Should().Contain("ApplyAsBackgroundCommand", "inline Images should apply selected background images");
        xaml.Should().Contain("PreviewMouseLeftButtonDown=\"InlineImagesList_PreviewMouseLeftButtonDown\"",
            "Images rows should arm drag without breaking thumbnail selection");
        xaml.Should().Contain("PreviewMouseMove=\"InlineImagesList_PreviewMouseMove\"",
            "Images rows should drag into the Preview background drop surface");
        xaml.Should().Contain("x:Name=\"CMenuImages\"", "Images source needs the FrmMain image context menu role");
        xaml.Should().Contain("x:Name=\"CMenuImages_AddItem\"", "Images menu should expose Add to Item");
        xaml.Should().Contain("ApplyToItemBackgroundCommand", "Add to Item should apply the selected image to the selected worship item");
        xaml.Should().Contain("x:Name=\"CMenuImages_AddDefault\"", "Images menu should expose Add to Default");
        xaml.Should().Contain("x:Name=\"CMenuImages_Refresh\"", "Images menu should expose Refresh Images Lists");

        xaml.Should().Contain("Tag=\"DefaultSource\"", "FrmMain keeps Default as a main-console source tab");
        xaml.Should().Contain("x:Name=\"DefPanel\"", "Default source should expose the legacy FrmMain DefPanel role inline");
        xaml.Should().Contain("Tag=\"DefPanel\"", "Default source should keep the legacy DefPanel tag for 1:1 UI inventory");
        xaml.Should().Contain("x:Name=\"DefgroupBox1\"", "DefPanel should surface the legacy text/default-format group");
        xaml.Should().Contain("x:Name=\"DefgroupBox2\"", "DefPanel should surface the legacy background/transition group");
        xaml.Should().Contain("x:Name=\"DefgroupBox3\"", "DefPanel should surface the legacy apply/reset group");
        xaml.Should().Contain("x:Name=\"Def_Head\"", "Default tab should expose FrmMain Def_Head");
        xaml.Should().Contain("ToggleLyricsTitleHeadingCommand", "Def_Head should connect to the real title heading toggle");
        xaml.Should().Contain("x:Name=\"Def_Outline\"", "Default tab should expose FrmMain Def_Outline");
        xaml.Should().Contain("ToggleLyricsOutlineCommand", "Def_Outline should connect to the real outline toggle");
        xaml.Should().Contain("x:Name=\"Def_Shadow\"", "Default tab should expose FrmMain Def_Shadow");
        xaml.Should().Contain("ToggleLyricsShadowCommand", "Def_Shadow should connect to the real shadow toggle");
        xaml.Should().Contain("x:Name=\"Def_Notations\"", "Default tab should expose FrmMain Def_Notations");
        xaml.Should().Contain("ToggleLyricsNotationsCommand", "Def_Notations should connect to the real chord display toggle");
        xaml.Should().Contain("x:Name=\"Def_ToZero\"", "Default tab should expose FrmMain Def_ToZero");
        xaml.Should().Contain("TransposeLiveResetCommand", "Def_ToZero should connect to the live transpose reset");
        xaml.Should().Contain("x:Name=\"Def_R1Align\"", "Default tab should expose Region1 alignment controls");
        xaml.Should().Contain("ApplyLyricsAlignmentCommand", "Def_R1Align should use the shared lyrics alignment command");
        xaml.Should().Contain("x:Name=\"Def_Region\"", "Default tab should expose region display controls");
        xaml.Should().Contain("ApplyRegionDisplayCommand", "Def_Region should use the shared region display command");
        xaml.Should().Contain("x:Name=\"Def_NoImage\"", "Default tab should expose FrmMain Def_NoImage");
        xaml.Should().Contain("ClearOutputBackgroundImageCommand", "Def_NoImage should clear the default output background");
        xaml.Should().Contain("ApplyBackgroundModeCommand", "Default tab should expose FrmMain background image modes");
        xaml.Should().Contain("x:Name=\"Def_Transition\"", "Default tab should expose transition defaults inline");
        xaml.Should().Contain("ToggleFadeTransitionCommand", "Default transition controls should toggle fade");
        xaml.Should().Contain("ApplyTransitionDurationCommand", "Default transition controls should expose duration presets");
        xaml.Should().Contain("ResetOutputAppearanceCommand", "Default tab restores the default output layout");
        xaml.Should().Contain("ApplyGlobalFormatToAllCommand", "Default tab can apply the current format globally");
        xaml.Should().Contain("ClearAllItemsFormattingCommand", "Default tab can clear item-specific formatting overrides");
    }

    [Fact]
    public void InlinePowerPointAndMediaSources_LazyLoadAndUseFileDropContract()
    {
        var code = CodeBehind;

        code.Should().Contain("EnsureInlineInfoScreenLoadedOnce(viewModel)", "InfoScr source tab should lazy-load on first selection");
        code.Should().Contain("InfoScreenSourceTab.DataContext = _inlineInfoScreens", "inline InfoScr tab should bind to its source VM");
        code.Should().Contain("typeof(InfoScreenSelection)", "InfoScr drags should use a typed payload, not arbitrary text");
        code.Should().Contain("typeof(InfoScreenSelection[])", "InfoScr multi-select drags should carry a typed selection array");
        code.Should().Contain("AddInlineInfoScreenSelectionToWorshipListAsync", "InfoScreen Enter/WL_Add/context menu should route through the multi-select add path");
        code.Should().Contain("InlineInfoScreenList.SelectedItems.OfType<InfoScreenSourceItem>()", "InfoScreen multi-select should use the actual selected WPF rows");
        code.Should().Contain("InlineInfoScreenList.SelectAll()", "InfoScreen CMenuFiles_SelectAll should select visible rows");
        code.Should().Contain("InlineInfoScreenList.UnselectAll()", "InfoScreen CMenuFiles_UnselectAll should clear visible rows");
        code.Should().Contain("await OpenInfoScreenEditorAsync(selection[0].Name).ConfigureAwait(true)",
            "InfoScreen CMenuFiles_Edit should open the selected saved InfoScreen in the editor like FrmMain EF_Edit_Clicked");
        code.Should().Contain("noticeViewModel.SelectedScreen = selectedScreenName",
            "InfoScreen Edit should seed the editor with the selected source item before opening");
        code.Should().Contain("await noticeViewModel.OpenCommand.ExecuteAsync(null).ConfigureAwait(true)",
            "InfoScreen Edit should load the selected saved screen through the same editor command users see in the modal");
        code.Should().Contain("ResolveInlineInfoScreenSourceFilesAsync(selection)",
            "InfoScreen CMenuFiles_Copy should prepare selected source files instead of staying a focus-only no-op");
        code.Should().Contain("store.PrepareCopySourceFileAsync(screen.Name, exportRoot)",
            "InfoScreen CMenuFiles_Copy should pass legacy .esi files through and export WPF JSON screens to copyable .esi files");
        code.Should().Contain("OpenExternalFileOperationWindow(ExternalFileItemKind.InfoScreen, ExternalFileOperationKind.Copy, sourceFiles)",
            "InfoScreen CMenuFiles_Copy should seed the FrmCopyMoveExternal replacement with selected .esi files");
        code.Should().Contain("Path.Combine(Path.GetTempPath(), \"EasislidesNext\", \"InfoScreenCopy\")",
            "InfoScreen file copy should keep WPF JSON exports available while the external copy dialog runs");
        code.Should().Contain("EnsureInlinePowerPointLoadedOnce(viewModel)", "PowerPoint source tab should lazy-load on first selection");
        code.Should().Contain("EnsureInlineMediaLoadedOnce(viewModel)", "Media source tab should lazy-load on first selection");
        code.Should().Contain("PowerPointSourceTab.DataContext = _inlinePowerPoint", "inline PowerPoint tab should bind to its library VM");
        code.Should().Contain("_services.GetService<IPowerPointRenderService>()", "inline PowerPoint preview mode should reuse the shared cached renderer");
        code.Should().Contain("AddInlinePowerPointSelectionToWorshipList", "PowerPoint Enter/WL_Add/context menu should route through the multi-select add path");
        code.Should().Contain("SelectedItems.OfType<PowerPointFileItem>()", "PowerPoint multi-select should use the actual selected WPF rows");
        code.Should().Contain("GetActivePowerPointList().SelectAll()", "PowerPoint CMenuFiles_SelectAll should target the visible list/preview surface");
        code.Should().Contain("flowLayoutExternalPowerPoint.UnselectAll()", "PowerPoint CMenuFiles_UnselectAll should clear preview-flow selection too");
        code.Should().Contain("MediaSourceTab.DataContext = _inlineMedia", "inline Media tab should bind to its library VM");
        code.Should().Contain("AddInlineMediaSelectionToWorshipList", "Media Enter/WL_Add/context menu should route through the multi-select add path");
        code.Should().Contain("InlineMediaList.SelectedItems.OfType<MediaFileItem>()", "Media multi-select should use the actual selected WPF rows");
        code.Should().Contain("InlineMediaList.SelectAll()", "CMenuFiles_SelectAll should select the visible MediaList rows");
        code.Should().Contain("InlineMediaList.UnselectAll()", "CMenuFiles_UnselectAll should clear the visible MediaList selection");
        code.Should().Contain("PreviewToLiveCommand.ExecuteAsync(null)", "CMenuFiles_AddShow should send the newly-added selection live when possible");
        code.Should().Contain("selection.Select(file => file.FilePath).ToArray()", "Media drag should carry every selected file like FrmMain MediaList.ItemDrag");
        code.Should().Contain("LaunchExternalFileForEdit(selection[0].FilePath, \"Media\")",
            "Media CMenuFiles_Edit should shell-open the selected file like FrmMain media file edit");
        code.Should().Contain("OpenExternalFileOperationWindow(ExternalFileItemKind.Media, ExternalFileOperationKind.Copy, selection.Select(file => file.FilePath))",
            "Media CMenuFiles_Copy should seed the FrmCopyMoveExternal replacement with selected media files");
        code.Should().Contain("sender is ListBox powerPointList", "PowerPoint list and preview-list drags should use the actual source surface");
        code.Should().Contain("ItemsControl.ContainerFromElement(powerPointList, source)", "PowerPoint drag arming should work for both list and preview style");
        code.Should().Contain("DragDrop.DoDragDrop(", "PowerPoint drag payload should still use WPF drag/drop");
        code.Should().Contain("            powerPointList,",
            "PowerPoint drag payload should originate from whichever PP_ListType surface is active");
        code.Should().Contain("LaunchExternalFileForEdit(selection[0].FilePath, \"PowerPoint\")",
            "PowerPoint CMenuFiles_Edit should open the selected file like FrmMain PP_Edit_Clicked");
        code.Should().Contain("OpenExternalFileOperationWindow(ExternalFileItemKind.PowerPoint, ExternalFileOperationKind.Copy, selection.Select(file => file.FilePath))",
            "PowerPoint CMenuFiles_Copy should seed the FrmCopyMoveExternal replacement with the selected PPT files");
        code.Should().Contain("viewModel.AddSourceFiles(sourceFiles)",
            "external-file copy should pass selected source files into the copy/move dialog instead of opening an empty shell");
        code.Should().Contain("Process.Start(new ProcessStartInfo(filePath)",
            "file Edit should shell-open the selected external file instead of being a focus-only no-op");
        code.Should().Contain("UseShellExecute = true",
            "external file Edit should use the Windows shell association like legacy Edit_Item");
        code.Should().Contain("EnsureInlineImageLoadedOnce(viewModel)", "Images source tab should lazy-load on first selection");
        code.Should().Contain("ImagesSourceTab.DataContext = _inlineImages", "inline Images tab should bind to ImageLibraryViewModel");
        code.Should().Contain("case \"ImagesSource\":", "Images source tab should participate in the shared Enter/add gesture route");
        code.Should().Contain("_inlineImages.ApplySelectedImageCommand.Execute(null)", "Images shared add gesture should follow FrmMain item-first/default-fallback semantics");
        code.Should().Contain("viewModel.SetSelectedItemBackgroundImageCommand.Execute(path)", "Images Add to Item should reuse the selected-item background command");
        code.Should().Contain("private ImageLibraryItem? _imageDragCandidate",
            "Images drags should carry the exact thumbnail item pressed, not a stale previous selection");
        code.Should().Contain("InlineImagesList_PreviewMouseMove",
            "Images source rows should start a drag after the normal WPF drag threshold");
        code.Should().Contain("InlineImagesList,",
            "Images source drag should originate from the inline Images list");
        code.Should().Contain("new DataObject(DataFormats.FileDrop, new[] { image.FilePath })",
            "Images source drags should reuse the same image-file payload accepted by PreviewArea_Drop");
        code.Should().Contain("DataFormats.FileDrop", "source drags must reuse the WorshipListPanel external file drop contract");
        code.Should().Contain("Path.Combine(workingFolder, \"Powerpoint\")", "PowerPoint should prefer the legacy working-folder source directory");
        code.Should().Contain("Path.Combine(workingFolder, \"Media\")", "Media should prefer the legacy working-folder source directory");
        code.Should().Contain("Path.Combine(workingFolder, \"Images\")", "Images should prefer the legacy working-folder Images directory");
    }

    [Fact]
    public void LowerLeftPane_ExposesInlinePraiseBookPeerOfWorshipList()
    {
        var xaml = Xaml;
        var code = CodeBehind;

        xaml.Should().Contain("x:Name=\"LeftListTabs\"", "lower-left FrmMain role split should be explicit");
        xaml.Should().Contain("Tag=\"WorshipList\"", "Worship List remains the first lower-left role");
        xaml.Should().Contain("AddSelectedSourceRequested=\"WorshipListPanel_AddSelectedSourceRequested\"",
            "FrmMain WL_Add should route through MainWindow because the active source tab lives there");
        xaml.Should().Contain("EditSelectedItemRequested=\"WorshipListPanel_EditSelectedItemRequested\"",
            "FrmMain CMenuWorship_Edit should route through MainWindow because the song editor is a modal shell window");
        xaml.Should().Contain("Tag=\"PraiseBook\"", "Praise Book should be a first-screen lower-left role");
        xaml.Should().Contain("x:Name=\"PraiseBookTab\"", "inline Praise Book tab needs a stable DataContext target");
        xaml.Should().Contain("x:Name=\"InlinePraiseBookSavedBooksCombo\"", "saved Praise Books must be loadable from the main shell");
        xaml.Should().Contain("Tag=\"PraiseBook\"", "PraiseBook combo role should remain explicit");
        xaml.Should().Contain("SelectionChanged=\"InlinePraiseBookSavedBooksCombo_SelectionChanged\"",
            "FrmMain PraiseBook_SelectedIndexChanged should open the selected praise book without requiring a separate Open click");
        xaml.Should().Contain("x:Name=\"PB_Manage\"", "Praise Book toolbar should expose FrmMain PB_Manage");
        xaml.Should().Contain("Tag=\"PB_Manage\"", "PB_Manage should keep the legacy toolbar role tag");
        xaml.Should().Contain("x:Name=\"PB_Add\"", "Praise Book toolbar should expose FrmMain PB_Add");
        xaml.Should().Contain("Tag=\"PB_Add\"", "PB_Add should keep the legacy toolbar role tag");
        xaml.Should().Contain("x:Name=\"PB_WordCount\"", "Praise Book toolbar should expose FrmMain PB_WordCount");
        xaml.Should().Contain("Tag=\"PB_WordCount\"", "PB_WordCount should keep the legacy toolbar role tag");
        xaml.Should().Contain("x:Name=\"PB_Delete\"", "Praise Book toolbar should expose FrmMain PB_Delete");
        xaml.Should().Contain("Tag=\"PB_Delete\"", "PB_Delete should keep the legacy toolbar role tag");
        xaml.Should().Contain("x:Name=\"PB_Word\"", "Praise Book toolbar should expose FrmMain PB_Word");
        xaml.Should().Contain("Tag=\"PB_Word\"", "PB_Word should keep the legacy toolbar role tag");
        xaml.Should().Contain("x:Name=\"PB_Html\"", "Praise Book toolbar should expose FrmMain PB_Html");
        xaml.Should().Contain("Tag=\"PB_Html\"", "PB_Html should keep the legacy toolbar role tag");
        xaml.Should().Contain("Grid.Column=\"8\"", "PB_Html should keep the FrmMain PB_Html button in the toolbar");
        CountOccurrences(SectionBetween(xaml, "<TabItem Tag=\"PraiseBook\"", "x:Name=\"PraiseBookItems\""), "<ColumnDefinition Width=\"Auto\" />")
            .Should().BeGreaterThanOrEqualTo(8, "PraiseBook toolbar columns must cover PB_Manage through PB_Html");
        xaml.Should().Contain("x:Name=\"PraiseBookItems\"", "Praise Book entries should render as a FrmMain-style ListView surface");
        xaml.Should().Contain("Tag=\"PraiseBookItems\"", "PraiseBookItems role should remain explicit");
        xaml.Should().Contain("ItemsSource=\"{Binding Entries}\"", "flat PraiseBookItems should bind to the current book entries");
        xaml.Should().Contain("SelectionChanged=\"PraiseBookItems_SelectionChanged\"", "selecting a PraiseBook row should refresh the Preview item like FrmMain PraiseBookListIndexChanged");
        xaml.Should().Contain("MouseDoubleClick=\"InlinePraiseBookItems_MouseDoubleClick\"", "double-clicking a PraiseBook item should add it to Worship List");
        xaml.Should().Contain("PreviewMouseLeftButtonDown=\"PraiseBookItems_PreviewMouseLeftButtonDown\"", "PraiseBook rows should arm drag without breaking selection");
        xaml.Should().Contain("PreviewMouseMove=\"PraiseBookItems_PreviewMouseMove\"", "PraiseBook rows should drag into Worship List at a target position");
        xaml.Should().Contain("PreviewMouseRightButtonDown=\"PraiseBookItems_PreviewMouseRightButtonDown\"", "right-clicking a PraiseBook row should target that row before opening the FrmMain menu");
        xaml.Should().Contain("x:Name=\"CMenuPraiseB\"", "PraiseBookItems should expose the FrmMain context menu");
        xaml.Should().Contain("x:Name=\"CMenuPraiseB_SelectAll\"", "PraiseBook context menu should expose Select All");
        xaml.Should().Contain("x:Name=\"CMenuPraiseB_UnselectAll\"", "PraiseBook context menu should expose Unselect All");
        xaml.Should().Contain("x:Name=\"CMenuPraiseB_Clear\"", "PraiseBook context menu should expose Clear PraiseBook List");
        xaml.Should().Contain("x:Name=\"CMenuPraiseB_Edit\"", "PraiseBook context menu should expose Edit item");

        code.Should().Contain("EnsureInlinePraiseBookLoadedOnce(viewModel)", "Praise Book tab should lazy-load on first selection");
        code.Should().Contain("PraiseBookTab.DataContext = _inlinePraiseBook", "inline Praise Book tab should bind to PraiseBookIndexViewModel");
        code.Should().Contain("InlinePraiseBookOpenBook_Click", "saved Praise Books should open from the inline tab");
        code.Should().Contain("InlinePraiseBookSavedBooksCombo_SelectionChanged", "selecting a saved PraiseBook should open it like FrmMain");
        code.Should().Contain("OpenSelectedInlinePraiseBookAsync", "combo selection and Open button should share one open path");
        code.Should().Contain("await _inlinePraiseBook.OpenBookCommand.ExecuteAsync(name)", "inline PraiseBook selection should use the real ViewModel open command");
        code.Should().Contain("InlinePraiseBookAddSelected_Click", "PB_Add should add the selected Folders song to the inline PraiseBook");
        code.Should().Contain("InlinePraiseBookDeleteSelected_Click", "PB_Delete should remove selected PraiseBook rows");
        code.Should().Contain("PraiseBookItems_SelectionChanged", "PraiseBook selection should update Preview without requiring double-click");
        code.Should().Contain("PreviewPraiseBookSongAsync", "PraiseBook selection preview should reuse the same DB/song resolution as add and drag");
        code.Should().Contain("CMenuPraiseB_Opened", "PraiseBook context menu should refresh enabled states on open");
        code.Should().Contain("viewModel.AddPraiseBookSong(entry.Title, entry.Number, entry.SongId)", "inline Praise Book entries should reuse the existing add path");
        code.Should().Contain("new DataObject(typeof(PraiseBookIndexEntry), entry)", "inline PraiseBook drag should carry the exact selected legacy entry");
        code.Should().Contain("private void PraiseBookItems_PreviewMouseRightButtonDown", "PraiseBook context menu should select the row under the pointer");
        code.Should().Contain("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A", "PraiseBook Ctrl+A should select all entries like FrmMain");
        code.Should().Contain("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete", "PraiseBook Delete should remove selected entries like FrmMain");
        code.Should().Contain("DeleteSelectedPraiseBookEntriesAsync", "PB_Delete and Delete key should share one removal path");
        code.Should().Contain("WorshipListPanel_AddSelectedSourceRequested", "WL_Add should be handled in the MainWindow shell");
        code.Should().Contain("WorshipListPanel_EditSelectedItemRequested", "CMenuWorship_Edit should be handled in the MainWindow shell");
        code.Should().Contain("OpenSelectedWorshipSongEditorAsync", "Edit item should open the selected DB song in SongEditorWindow");
        code.Should().Contain("UpdateSelectedSongQueueItem", "saving the editor should refresh the Worship List row");
        code.Should().Contain("AddSelectedSourceToWorshipListAsync", "WL_Add should dispatch by active source tab");
        code.Should().Contain("GetRequiredService<IPraiseBookStore>()", "inline and modal PraiseBook surfaces should use the DI store wired to the legacy working folder");
        code.Should().NotContain("new PraiseBookStore()", "direct construction falls back to AppData and loses C:\\EasiSlides legacy .esp files");
        AppCode.Should().Contain("services.AddSingleton<IPraiseBookStore, PraiseBookStore>()", "DI must provide the WorkingFolder-aware PraiseBook store at runtime");
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

        xaml.Should().Contain("{Binding CopyPreviewToOutputCommand}", "FrmMain btnToOutput should copy Preview into Output without starting live");
        xaml.Should().Contain("{Binding CopyPreviewToOutputAndNextCommand}", "FrmMain btnToOutputMoveNext should copy Output and advance Preview without starting live");
        xaml.Should().Contain("{Binding PreviewToLiveCommand}", "FrmMain btnToLive should use PreviewItem->OutputItem live semantics, separate from the fixed operator GoLive command");
        xaml.Should().MatchRegex(@"(?s)<Button\s+x:Name=""btnToLive""[^>]*>\s*<ui:SymbolIcon",
            "FrmMain Preview top command strip uses compact icon buttons rather than text-only toolbar labels");
        xaml.Should().MatchRegex(@"(?s)<Button\s+x:Name=""btnToOutputMoveNext""[^>]*>\s*<ui:SymbolIcon",
            "FrmMain Preview copy-and-next command should remain a compact icon button");
        xaml.Should().MatchRegex(@"(?s)<Button\s+x:Name=""btnToOutput""[^>]*>\s*<ui:SymbolIcon",
            "FrmMain Preview copy-to-Output command should remain a compact icon button");
        OperatorBarXaml.Should().Contain("{Binding SendToOutputAndNextCommand}", "fixed operator bar keeps F11 live send-and-next");
        xaml.Should().Contain("Text=\"{Binding DataContext.OutputItem.Title, RelativeSource={RelativeSource AncestorType=ListView}, TargetNullValue='Output'}\"",
            "Output title should follow the prepared OutputItem, not the selected Preview item");
        xaml.Should().Contain("{Binding ToggleOutputBlackCommand}", "Output strip exposes FrmMain-style checked Black toggle");
        xaml.Should().Contain("IsChecked=\"{Binding IsOutputBlackActive, Mode=OneWay}\"", "Black toggle should reflect the current live blackout state");
        xaml.Should().Contain("{Binding ToggleOutputClearCommand}", "Output strip exposes FrmMain-style checked Clear toggle");
        xaml.Should().Contain("IsChecked=\"{Binding IsOutputClearActive, Mode=OneWay}\"", "Clear toggle should reflect the current live clear state");
        xaml.Should().Contain("{Binding ToggleOutputLiveCommand}", "Output strip exposes FrmMain-style checked GoLive toggle");
        xaml.Should().Contain("IsChecked=\"{Binding IsOutputLiveActive, Mode=OneWay}\"", "GoLive toggle should reflect the current live active state");
        xaml.Should().Contain("x:Name=\"ClassicPreviewPositionLabel\"", "Preview strip should expose the current Preview page/slide position");
        xaml.Should().Contain("Text=\"{Binding PreviewNavigationPositionLabel}\"", "Preview position should follow Preview navigation only");
        xaml.Should().Contain("x:Name=\"ClassicOutputPositionLabel\"", "Output strip should expose the current Output page/slide position");
        xaml.Should().Contain("Text=\"{Binding OutputNavigationPositionLabel}\"", "Output position should follow prepared/live Output navigation only");
    }

    [Fact]
    public void ClassicRightPanes_UseFrmMainFlatSplitPanelChrome()
    {
        var xaml = Xaml;

        foreach (var pane in new[]
        {
            "ClassicPreviewPane",
            "ClassicOutputPane",
        })
        {
            var section = SectionBetween(xaml, $"x:Name=\"{pane}\"", "Padding=\"4\">");
            section.Should().Contain("Background=\"White\"",
                $"{pane} is the white top half of the FrmMain split panel, not a modern card");
            section.Should().Contain("BorderThickness=\"1\"", $"{pane} keeps the WinForms-like splitter border");
            section.Should().NotContain("CornerRadius=", $"{pane} should stay square like FrmMain panels");
        }

        foreach (var pane in new[]
        {
            "ClassicPreviewSlidePane",
            "ClassicOutputSlidePane",
        })
        {
            var section = SectionBetween(xaml, $"x:Name=\"{pane}\"", "Padding=\"6\">");
            section.Should().Contain("Background=\"#808080\"",
                $"{pane} keeps the legacy grey lower screen work area");
            section.Should().Contain("BorderThickness=\"1\"", $"{pane} keeps the WinForms-like splitter border");
            section.Should().NotContain("CornerRadius=", $"{pane} should stay square like FrmMain panels");
        }
    }

    [Fact]
    public void ClassicPreviewAndOutputBottomStrips_UseFrmMainWhiteToolbarRows()
    {
        var xaml = Xaml;

        foreach (var pair in new[]
        {
            ("ClassicPreviewBottomStripHost", "ClassicPreviewBottomStrip"),
            ("ClassicOutputBottomStripHost", "ClassicOutputBottomStrip"),
        })
        {
            var host = SectionBetween(xaml, $"x:Name=\"{pair.Item1}\"", $"</Border>");
            host.Should().Contain("Background=\"White\"",
                $"{pair.Item1} should separate controls from the grey screen work area like FrmMain");
            host.Should().Contain("BorderThickness=\"0,0,0,1\"",
                $"{pair.Item1} should draw the thin divider above the grey screen work area");
            host.Should().Contain($"x:Name=\"{pair.Item2}\"",
                $"{pair.Item2} should remain inside the white toolbar row");
        }
    }

    [Fact]
    public void ClassicPreviewAndOutputPanes_ExposeFrmMainControlRoles()
    {
        var xaml = Xaml;

        foreach (var role in new[]
        {
            "PreviewPanelDisplayName",
            "PreviewInfo",
            "flowLayoutPreviewPowerPoint",
            "PreviewHolder",
            "OutputPanelDisplayName",
            "OutputInfo",
            "flowLayoutOutputPowerPoint",
            "OutputHolder",
            "OutputBack",
        })
        {
            xaml.Should().Contain($"Tag=\"{role}\"", $"{role} should remain explicitly mapped to the FrmMain control role");
        }

        xaml.Should().Contain("x:Name=\"ClassicPreviewPowerPointThumbnailGrid\"",
            "Preview top pane should expose the PowerPoint thumbnail strip separately from Output");
        xaml.Should().Contain("x:Name=\"ClassicPreviewPowerPointSurface\"",
            "Preview top pane should expose a named FrmMain-style PowerPoint thumbnail surface");
        var previewPowerPointSurface = SectionBetween(xaml, "x:Name=\"ClassicPreviewPowerPointSurface\"", "</ScrollViewer>");
        previewPowerPointSurface.Should().Contain("Tag=\"flowLayoutPreviewPowerPoint\"",
            "Preview PowerPoint top pane should keep the legacy flowLayoutPreviewPowerPoint role on the scroller surface");
        previewPowerPointSurface.Should().Contain("<WrapPanel />",
            "Preview PowerPoint top pane should wrap thumbnails like FrmMain flowLayoutPreviewPowerPoint");
        previewPowerPointSurface.Should().Contain("Width=\"180\"",
            "Preview PowerPoint thumbnails should use the same operator-sized card as Output thumbnails");
        previewPowerPointSurface.Should().Contain("Height=\"102\"",
            "Preview PowerPoint thumbnails should preserve 16:9 cards instead of the older tiny strip");
        previewPowerPointSurface.Should().Contain("<Setter Property=\"BorderBrush\" Value=\"Red\" />",
            "the current Preview PowerPoint thumbnail should use the FrmMain red selected border");
        previewPowerPointSurface.Should().Contain("Background=\"#FFF6E600\"",
            "Preview PowerPoint thumbnails should show the FrmMain yellow slide-number badge");
        previewPowerPointSurface.Should().NotContain("PowerPoint.PreviewImage",
            "the top Preview pane should be thumbnails only; the large slide belongs to ClassicPreviewHolder");
        xaml.Should().Contain("ItemsSource=\"{Binding PreviewLyricsPages}\"",
            "Preview lyrics pane should expose FrmMain-style page cards instead of a raw single lyrics block");
        xaml.Should().Contain("GoToPreviewLyricsPageCommand",
            "Preview lyrics page card clicks should move only the selected Preview item");
        var previewLyricsSurface = SectionBetween(xaml, "x:Name=\"flowLayoutPreviewLyrics\"", "x:Name=\"IndPanel\"");
        previewLyricsSurface.Should().Contain("<WrapPanel />",
            "Preview lyrics cards should use a FrmMain flow layout rather than a single-column modern list");
        previewLyricsSurface.Should().Contain("Width=\"180\"",
            "Preview lyrics cards should visually match Output page cards");
        previewLyricsSurface.Should().Contain("<Setter Property=\"BorderBrush\" Value=\"Red\" />",
            "the current Preview lyrics page should use the FrmMain red selected border");
        previewLyricsSurface.Should().Contain("Background=\"#FFF6E600\"",
            "Preview lyrics cards should keep the FrmMain yellow page-number badge");
        xaml.Should().Contain("x:Name=\"ClassicOutputPowerPointSurface\"",
            "Output top pane should expose the live PowerPoint thumbnail/list surface");
        xaml.Should().Contain("x:Name=\"ClassicOutputHolder\"",
            "Output bottom pane should expose the large output holder surface");
        xaml.Should().Contain("x:Name=\"ClassicOutputBack\"",
            "Output bottom pane should expose the large output background surface");
        xaml.Should().Contain("x:Key=\"ClassicPanelDisplayList\"",
            "PreviewPanelDisplayName and OutputPanelDisplayName should render as FrmMain-style list rows, not plain labels");
        xaml.Should().Contain("Style=\"{StaticResource ClassicPanelDisplayList}\"",
            "both panel display-name controls should use the legacy list-row visual treatment");
        xaml.Should().Contain("SelectedIndex=\"0\"",
            "the single display-name row should stay selected like the legacy ListView item");
        xaml.Should().Contain("Text=\"{Binding DataContext.SelectedItem.Title, RelativeSource={RelativeSource AncestorType=ListView}, TargetNullValue='Preview'}\"",
            "Preview title row should bind to the selected Preview item through the list control");
        xaml.Should().Contain("Text=\"{Binding DataContext.PreviewPanelSourceText, RelativeSource={RelativeSource AncestorType=ListView}}\"",
            "Preview title row should expose a FrmMain-style source/type cell");
        xaml.Should().Contain("Text=\"{Binding DataContext.PreviewPanelStatusText, RelativeSource={RelativeSource AncestorType=ListView}}\"",
            "Preview title row should expose a FrmMain-style status/position cell");
        xaml.Should().Contain("Text=\"{Binding DataContext.OutputItem.Title, RelativeSource={RelativeSource AncestorType=ListView}, TargetNullValue='Output'}\"",
            "Output title row should bind to the prepared/live Output item through the list control");
        xaml.Should().Contain("Text=\"{Binding DataContext.OutputPanelSourceText, RelativeSource={RelativeSource AncestorType=ListView}}\"",
            "Output title row should expose a FrmMain-style source/type cell");
        xaml.Should().Contain("Text=\"{Binding DataContext.OutputPanelStatusText, RelativeSource={RelativeSource AncestorType=ListView}}\"",
            "Output title row should expose a FrmMain-style status/live cell");
    }

    [Fact]
    public void ClassicPreviewAndOutputPanes_ExposeFrmMainOperatorButtonsAndLiveMessage()
    {
        var xaml = Xaml;
        var code = CodeBehind;
        var outputTextBoxStart = xaml.IndexOf("x:Name=\"OutputTextBoxLM\"", StringComparison.Ordinal);
        outputTextBoxStart.Should().BeGreaterThanOrEqualTo(0, "OutputTextBoxLM should remain mapped to FrmMain");
        var outputTextBoxEnd = xaml.IndexOf("</TextBox>", outputTextBoxStart, StringComparison.Ordinal);
        outputTextBoxEnd.Should().BeGreaterThan(outputTextBoxStart, "OutputTextBoxLM should host Enter key input bindings");
        var outputTextBoxXaml = xaml[outputTextBoxStart..outputTextBoxEnd];

        foreach (var name in new[]
        {
            "btnToLive",
            "btnToOutputMoveNext",
            "btnToOutput",
            "IndcbPreviewNotes",
            "IndradioButtonText",
            "IndradioButtonFormat",
            "IndradioButtonInfo",
            "panelIndTemplate",
            "toolStripIndTemplates",
            "Ind_LoadTemplate",
            "Ind_SaveTemplate",
            "Ind_BackColour",
            "Ind_BackImageSelect",
            "Ind_NoImage",
            "PreviewBtnItemUp",
            "PreviewBtnItemDown",
            "PreviewBtnSlideUp",
            "PreviewBtnSlideDown",
            "ClassicPreviewPositionLabel",
            "flowLayoutPanel1",
            "PreviewBtnVerse1",
            "PreviewBtnVerseChorus",
            "PreviewBtnVerseEnding",
            "cbOutputBlack",
            "cbOutputClear",
            "cbGoLive",
            "OutputBtnJumpToNonRotate",
            "OutputBtnMedia",
            "OutputBtnRefAlert",
            "OutputBtnItemUp",
            "OutputBtnItemDown",
            "OutputBtnSlideUp",
            "OutputBtnSlideDown",
            "ClassicOutputPositionLabel",
            "flowLayoutPanel2",
            "OutputBtnVerse1",
            "OutputBtnVerseChorus",
            "OutputBtnVerseEnding",
            "panelOutputLM1",
            "panelOutputLM2",
            "panelOutputLM3",
            "OutputTextBoxLM",
            "OutputBtnLMSend",
            "OutputBtnLMClear",
        })
        {
            xaml.Should().Contain($"x:Name=\"{name}\"", $"{name} should remain mapped to the FrmMain operator control");
        }

        xaml.Should().Contain("Text=\"{Binding OutputLiveMessage, UpdateSourceTrigger=PropertyChanged}\"",
            "OutputTextBoxLM should edit the ViewModel live-message text");
        outputTextBoxXaml.Should().Contain("Key=\"Return\"",
            "FrmMain OutputTextBoxLM should send the live message when the operator presses Enter");
        outputTextBoxXaml.Should().Contain("Command=\"{Binding SendLiveMessageCommand}\"",
            "OutputTextBoxLM Enter should use the same send command as FrmMain");
        xaml.Should().Contain("Command=\"{Binding SendLiveMessageCommand}\"",
            "OutputBtnLMSend should send the live message");
        xaml.Should().Contain("Command=\"{Binding ClearLiveMessageCommand}\"",
            "OutputBtnLMClear should clear the live message");
        xaml.Should().Contain("x:Name=\"IndcbPreviewNotes\"", "IndcbPreviewNotes should remain mapped to the FrmMain Preview notes button");
        xaml.Should().Contain("Click=\"OpenSessionNotes_Click\"",
            "IndcbPreviewNotes should open the same session notes editor as Tools and WL_Notes");
        xaml.Should().Contain("Command=\"{Binding ShowPreviewTextModeCommand}\"",
            "IndradioButtonText should switch the Preview top pane to the FrmMain text surface");
        xaml.Should().Contain("Command=\"{Binding ShowPreviewFormatModeCommand}\"",
            "IndradioButtonFormat should switch the Preview top pane to the FrmMain IndPanel formatting surface");
        xaml.Should().Contain("Command=\"{Binding ShowPreviewInfoModeCommand}\"",
            "IndradioButtonInfo should switch the Preview top pane to the FrmMain PreviewInfo surface");
        xaml.Should().Contain("IsChecked=\"{Binding IsPreviewTextMode, Mode=OneWay}\"",
            "the Text mode button should reflect the active Preview panel mode");
        xaml.Should().Contain("IsChecked=\"{Binding IsPreviewFormatMode, Mode=OneWay}\"",
            "the Set mode button should reflect the active Preview panel mode");
        xaml.Should().Contain("IsChecked=\"{Binding IsPreviewInfoMode, Mode=OneWay}\"",
            "the Info mode button should reflect the active Preview panel mode");
        xaml.Should().Contain("x:Name=\"flowLayoutPreviewLyrics\"",
            "the Preview text mode should expose the legacy lyrics surface role");
        xaml.Should().Contain("x:Name=\"IndPanel\"",
            "the Preview Set mode should expose the legacy individual-format panel role");
        xaml.Should().Contain("x:Name=\"Ind_checkBox\"",
            "the Preview Set mode should expose the legacy individual-format checkbox");
        xaml.Should().Contain("Visibility=\"{Binding IsPreviewTextMode, Converter={StaticResource BoolToVis}}\"",
            "only the text surface should show in Text mode");
        xaml.Should().Contain("Visibility=\"{Binding IsPreviewFormatMode, Converter={StaticResource BoolToVis}}\"",
            "only the format surface should show in Set mode");
        xaml.Should().Contain("Visibility=\"{Binding IsPreviewInfoMode, Converter={StaticResource BoolToVis}}\"",
            "only the info surface should show in Info mode");
        xaml.Should().Contain("Command=\"{Binding ToggleUseIndividualFormattingCommand}\"",
            "Ind_checkBox should toggle the selected item's individual formatting");
        xaml.Should().Contain("Command=\"{Binding SetSelectedItemTextColorCommand}\"",
            "IndPanel should expose item-specific text color controls");
        xaml.Should().Contain("Command=\"{Binding ClearSelectedItemFormattingCommand}\"",
            "IndPanel should expose item-specific formatting clear");
        xaml.Should().Contain("Click=\"LoadPreviewItemTemplate_Click\"",
            "Ind_LoadTemplate should load FrmMain .est individual settings templates");
        xaml.Should().Contain("Click=\"SavePreviewItemTemplate_Click\"",
            "Ind_SaveTemplate should save FrmMain .est individual settings templates");
        code.Should().Contain("IndividualFormatTemplateFile.LoadFormatData(dialog.FileName)",
            "Preview Set template load should read ListHeader/FormatData from legacy .est files");
        code.Should().Contain("viewModel.ApplySelectedItemFormatDataTemplate(formatData)",
            "Preview Set template load should apply raw FormatData to the selected item");
        code.Should().Contain("IndividualFormatTemplateFile.SaveFormatData(dialog.FileName, viewModel.SelectedItemFormatData)",
            "Preview Set template save should write the selected item's raw FormatData");
        xaml.Should().Contain("Text=\"{Binding SelectedItemBackgroundImagePath, Mode=OneWay}\"",
            "IndPanel should show the current selected-item background image path");
        xaml.Should().Contain("Click=\"SelectPreviewItemBackgroundImage_Click\"",
            "IndPanel should let operators choose a per-item background image from the first-screen Preview Set panel");
        xaml.Should().Contain("Command=\"{Binding SetSelectedItemBackgroundImageCommand}\"",
            "Ind_NoImage should clear the selected item's background image through the same VM command as the Worship List menu");
        code.Should().Contain("private void SelectPreviewItemBackgroundImage_Click",
            "Preview Set item-image selection should be handled by the main shell view");
        code.Should().Contain("viewModel.SetSelectedItemBackgroundImage(dialog.FileName)",
            "Preview Set image selection should write FormatData code 61 via the verified VM method");
        xaml.Should().Contain("Text=\"{Binding PreviewItemInfoText}\"",
            "PreviewInfo should show selected Preview item metadata instead of remaining blank");
        xaml.Should().Contain("Command=\"{Binding JumpToNextNonRotateOutputItemCommand}\"",
            "OutputBtnJumpToNonRotate should jump within the independent Output/live queue context");
        xaml.Should().NotContain("x:Name=\"OutputBtnJumpToNonRotate\"\r\n                                Tag=\"OutputBtnJumpToNonRotate\"\r\n                                Style=\"{StaticResource EsButton.Secondary}\"\r\n                                MinWidth=\"30\"\r\n                                MinHeight=\"28\"\r\n                                Padding=\"6,0\"\r\n                                Margin=\"0,0,4,0\"\r\n                                Content=\"J\"\r\n                                IsEnabled=\"False\"",
            "OutputBtnJumpToNonRotate must not remain a disabled placeholder");
        xaml.Should().Contain("Command=\"{Binding PlayOutputMediaCommand}\"",
            "OutputBtnMedia should use the current Output/live item rather than the selected Preview media");
        xaml.Should().NotContain("x:Name=\"OutputBtnMedia\"\r\n                                Tag=\"OutputBtnMedia\"\r\n                                Command=\"{Binding Media.PlayPauseCommand}\"",
            "OutputBtnMedia must not fall back to Preview-loaded media playback");
        xaml.Should().Contain("x:Name=\"OutputBtnRefAlert\"",
            "Output reference alert button should keep its FrmMain control name");
        xaml.Should().Contain("Command=\"{Binding ToggleOutputReferenceAlertCommand}\"",
            "OutputBtnRefAlert should toggle the live reference alert instead of staying a disabled placeholder");
        xaml.Should().Contain("x:Key=\"ClassicVerseJumpButton\"",
            "FrmMain section buttons should have a dedicated style that can hide unavailable sections");
        xaml.Should().Contain("<Setter Property=\"Visibility\" Value=\"Collapsed\" />",
            "Unavailable section buttons should collapse like FrmMain Visible=false verse buttons");
        xaml.Should().Contain("x:Name=\"PreviewBtnVerse1\" Tag=\"1\" Command=\"{Binding JumpToLyricsSectionCommand}\"",
            "Preview verse buttons should keep moving the selected Preview item");
        xaml.Should().Contain("x:Name=\"PreviewBtnVerse1\" Tag=\"1\" Command=\"{Binding JumpToLyricsSectionCommand}\" CommandParameter=\"1\" Style=\"{StaticResource ClassicVerseJumpButton}\"",
            "Preview verse buttons should hide when the selected Preview item does not expose that section");
        xaml.Should().MatchRegex("x:Name=\"PreviewBtnItemUp\"\\s+Tag=\"PreviewBtnItemUp\"\\s+Command=\"\\{Binding PreviousPreviewItemCommand\\}\"",
            "Preview item-up should move only the selected Preview item and not publish live Output");
        xaml.Should().MatchRegex("x:Name=\"PreviewBtnItemDown\"\\s+Tag=\"PreviewBtnItemDown\"\\s+Command=\"\\{Binding NextPreviewItemCommand\\}\"",
            "Preview item-down should move only the selected Preview item and not publish live Output");
        xaml.Should().NotMatchRegex("x:Name=\"PreviewBtnItemDown\"\\s+Tag=\"PreviewBtnItemDown\"\\s+Command=\"\\{Binding NextItemCommand\\}\"",
            "Preview item buttons must not use the global live-aware item command");
        xaml.Should().MatchRegex("x:Name=\"PreviewBtnSlideUp\"\\s+Tag=\"PreviewBtnSlideUp\"\\s+Command=\"\\{Binding PreviousSlideCommand\\}\"",
            "Preview slide-up should use the item-type-aware Preview slide/page command");
        xaml.Should().MatchRegex("x:Name=\"PreviewBtnSlideDown\"\\s+Tag=\"PreviewBtnSlideDown\"\\s+Command=\"\\{Binding NextSlideCommand\\}\"",
            "Preview slide-down should use the item-type-aware Preview slide/page command");
        xaml.Should().NotMatchRegex("x:Name=\"PreviewBtnSlideDown\"\\s+Tag=\"PreviewBtnSlideDown\"\\s+Command=\"\\{Binding NextLyricsPageCommand\\}\"",
            "Preview slide buttons must not bypass PPT slide navigation");
        xaml.Should().Contain("x:Name=\"OutputBtnVerse1\" Tag=\"1\" Command=\"{Binding JumpToOutputLyricsSectionCommand}\"",
            "Output verse buttons should move the live Output item independently from Preview");
        xaml.Should().Contain("x:Name=\"OutputBtnVerse1\" Tag=\"1\" Command=\"{Binding JumpToOutputLyricsSectionCommand}\" CommandParameter=\"1\" Style=\"{StaticResource ClassicVerseJumpButton}\"",
            "Output verse buttons should hide when the prepared/live Output item does not expose that section");
        xaml.Should().NotContain("x:Name=\"OutputBtnVerse1\" Tag=\"1\" Command=\"{Binding JumpToLyricsSectionCommand}\"",
            "Output verse buttons must not be wired back to the selected Preview item");
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
    [InlineData("InputGestureText=\"F8\"")]      // Preview를 Output으로 보내기(FrmMain 글로벌)
    [InlineData("InputGestureText=\"F7\"")]      // Preview를 Output으로 보내고 Black 해제(FrmMain 글로벌)
    [InlineData("InputGestureText=\"Space\"")]   // 다음 슬라이드/절
    [InlineData("InputGestureText=\"Shift+Space\"")] // 이전 슬라이드/절
    [InlineData("InputGestureText=\"F9\"")]      // 검은 화면
    [InlineData("InputGestureText=\"F3\"")]      // 화면 비우기
    [InlineData("InputGestureText=\"F1\"")]      // 도움말
    [InlineData("InputGestureText=\"Ctrl+R\"")]  // 현재 항목 처음으로(증분136)
    [InlineData("InputGestureText=\"Ctrl+F5\"")] // 출력 새로고침(증분136)
    public void MenuBar_ShowsCoreGestureHints(string gesture)
        => Xaml.Should().Contain(gesture, $"메뉴에 {gesture} 단축키 힌트가 보여야 함");

    [Theory]
    // 메뉴 힌트 문자열이 실제 카탈로그 단축키와 같은 메뉴 항목의 VM 명령에 일치해야 한다(거짓 힌트·잘못된 항목 부착 방지).
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveGo, "F12", "ToggleOutputLiveCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveGoAndNext, "F11", "SendToOutputAndNextCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LivePreviewToOutput, "F8", "CopyPreviewToOutputShortcutCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LivePreviewToOutputClearBlack, "F7", "CopyPreviewToOutputAndClearBlackCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveNext, "Space", "LiveNextShortcutCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LivePrevious, "Shift+Space", "LivePreviousShortcutCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveBlack, "F9", "ToggleOutputBlackCommand")]
    [InlineData(Easislides.Wpf.Shell.MainCommandIds.LiveClear, "F3", "ToggleOutputClearCommand")]
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

    [Theory]
    [InlineData("ToggleOutputLiveCommand", "IsOutputLiveActive")]
    [InlineData("ToggleOutputBlackCommand", "IsOutputBlackActive")]
    [InlineData("ToggleOutputClearCommand", "IsOutputClearActive")]
    public void OutputLiveSafetyMenuItems_AreCheckedFrmMainToggles(string command, string activeProperty)
    {
        var menuItem = MenuItemOpeningFor(command);

        menuItem.Should().Contain("IsCheckable=\"True\"", $"{command} 메뉴는 FrmMain CheckBox/Menu checked 상태를 보여야 함");
        menuItem.Should().Contain($"IsChecked=\"{{Binding {activeProperty}, Mode=OneWay}}\"",
            $"{command} 메뉴는 {activeProperty} 상태를 반영해야 함");
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
