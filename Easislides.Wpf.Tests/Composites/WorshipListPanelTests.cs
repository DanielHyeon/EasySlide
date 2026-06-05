using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Input;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// WorshipListPanel 컴포지트 추출 동등성 가드 — 계획서 §5.2 / 작업 A-5 (라이브 경로).
///
/// MainWindow 좌측 "예배 순서" 카드(제목 + 송출 큐 ListBox)를 재사용 UserControl로 분리했다.
/// 라이브 송출 경로이므로 동작 동등성이 최우선:
///  1) 컴포지트가 송출 큐 바인딩(Queue/SelectedItem TwoWay/Title) + 접근성 이름을 그대로 보유.
///  2) MainWindow 는 그 ListBox 를 더 이상 인라인하지 않고 컴포지트를 column 0 에 호스트.
///
/// ViewModel(MainViewModel)은 변경하지 않았으므로 동작 동등성은 기존 MainViewModelTests
/// (Queue/SelectedItem 동작)가 계속 보장한다. (정적 XAML 구조 검증.)
/// </summary>
public class WorshipListPanelTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    private static string LoadText(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return File.ReadAllText(Path.Combine(repoRoot, relativePath));
    }

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    [Fact]
    public void Composite_Retains_Queue_ListBox_Binding_Surface()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/WorshipListPanel.xaml");

        var list = composite.Descendants().Single(
            e => e.Name.LocalName == "ListView" && Attr(e, "Name") == "QueueList");
        Attr(list, "ItemsSource").Should().Contain("Queue");
        Attr(list, "SelectedItem").Should().Contain("SelectedItem").And.Contain("TwoWay");
        Attr(list, "SelectionMode").Should().Be("Extended", "FrmMain WorshipListItems supports multi-select for Select All/Unselect All");
        Attr(list, "AutomationProperties.Name").Should().Be("예배 순서 목록");
        Attr(list, "Tag").Should().Be("WorshipListItems", "WPF queue should keep the FrmMain ListView role visible");
        Attr(list, "PreviewMouseRightButtonDown").Should().Be("QueueList_PreviewMouseRightButtonDown",
            "right-click should select the row under the mouse before context-menu commands run");

        // 증분137 — 항목 한 줄은 종류 아이콘 + 제목. 종류 아이콘은 Kind→Fluent 변환기로 바인딩.
        var symbolIcon = list.Descendants().Single(e => e.Name.LocalName == "SymbolIcon");
        Attr(symbolIcon, "Symbol").Should().Contain("Kind").And.Contain("KindToSymbol", "종류 아이콘은 Kind 를 KindToSymbol 변환기로 바인딩");
        var titleBlock = list.Descendants().Single(
            e => e.Name.LocalName == "TextBlock" && (Attr(e, "Text")?.Contains("Title") ?? false));
        Attr(titleBlock, "Text").Should().Contain("Title", "제목은 TextBlock 에 그대로 바인딩");
        // 행(StackPanel)의 자동화 이름을 제목으로 고정 — 장식 아이콘 글리프 대신 제목만 읽힌다(a11y 무회귀).
        var rowPanel = list.Descendants().Single(
            e => e.Name.LocalName == "StackPanel" && (Attr(e, "AutomationProperties.Name")?.Contains("Title") ?? false));
        Attr(rowPanel, "AutomationProperties.Name").Should().Contain("Title");

        // 카드 제목 보존.
        composite.Descendants().Any(e => e.Name.LocalName == "TextBlock" && Attr(e, "Text") == "예배 순서")
            .Should().BeTrue("예배 순서 제목 보존");

        composite.Descendants().Any(e => e.Name.LocalName == "Grid" && Attr(e, "Name") == "ClassicWorshipListSessionStrip")
            .Should().BeTrue("SessionList row should remain a compact FrmMain-style strip");
        composite.Descendants().Any(e => e.Name.LocalName == "StackPanel" && Attr(e, "Name") == "ClassicWorshipListToolStrip2")
            .Should().BeTrue("WL_Up/WL_Down/WL_Delete commands should stay in the visible legacy tool strip");
        var wlAdd = composite.Descendants().Single(e => e.Name.LocalName == "Button" && Attr(e, "Name") == "WL_Add");
        Attr(wlAdd, "Click").Should().Be("WL_Add_Click", "FrmMain WL_Add should route the current source selection to Worship List");
        Attr(wlAdd, "AutomationProperties.Name").Should().Contain("예배 순서에 추가");
        var wlOpen = composite.Descendants().Single(e => e.Name.LocalName == "Button" && Attr(e, "Name") == "WL_Open");
        Attr(wlOpen, "Tag").Should().Be("WL_Open", "FrmMain WL_Open role should be visible in the lower-left toolbar");
        Attr(wlOpen, "Click").Should().Be("WL_Open_Click", "FrmMain WL_Open should open external files from the lower-left toolbar");
        Attr(wlOpen, "AutomationProperties.Name").Should().Contain("외부 파일");
        var wlWord = composite.Descendants().Single(e => e.Name.LocalName == "Button" && Attr(e, "Name") == "WL_Word");
        Attr(wlWord, "Tag").Should().Be("WL_Word", "FrmMain WL_Word role should be visible in the lower-left toolbar");
        Attr(wlWord, "Click").Should().Be("WL_Word_Click", "FrmMain WL_Word should generate an RTF document from the Worship List");
        Attr(wlWord, "ToolTip").Should().Be("Generate RTF Document");
        Attr(wlWord, "AutomationProperties.Name").Should().Contain("RTF");
        var wlNotes = composite.Descendants().Single(e => e.Name.LocalName == "Button" && Attr(e, "Name") == "WL_Notes");
        Attr(wlNotes, "Tag").Should().Be("WL_Notes", "FrmMain WL_Notes role should be visible in the lower-left toolbar");
        Attr(wlNotes, "Click").Should().Be("WL_Notes_Click", "FrmMain WL_Notes should open the session notes editor");
        Attr(wlNotes, "AutomationProperties.Name").Should().Contain("세션 메모");
        composite.Descendants().Any(e => e.Name.LocalName == "ComboBox" && Attr(e, "Tag") == "SessionList")
            .Should().BeTrue("saved worship list combo should retain the FrmMain SessionList role");
    }

    [Fact]
    public void ContextMenu_ExposesFrmMainWorshipCommands()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/WorshipListPanel.xaml");

        var menu = composite.Descendants().Single(
            e => e.Name.LocalName == "ContextMenu" && Attr(e, "Name") == "CMenuWorship");
        Attr(menu, "Tag").Should().Be("CMenuWorship");
        Attr(menu, "Opened").Should().Be("CMenuWorship_Opened");

        var expected = new[]
        {
            ("CMenuWorship_SelectAll", "Select All", "CMenuWorship_SelectAll_Click", ""),
            ("CMenuWorship_UnselectAll", "Unselect All", "CMenuWorship_UnselectAll_Click", ""),
            ("CMenuWorship_Clear", "Clear Worship List", "", "ClearWorshipListCommand"),
            ("CMenuWorship_Edit", "Edit item", "CMenuWorship_Edit_Click", ""),
            ("CMenuWorship_Play", "Play Media", "", "PlaySelectedWorshipMediaCommand"),
            ("CMenuWorship_PlayOnOutput", "Play Media on Output Monitor", "", "PlaySelectedWorshipMediaOnOutputCommand"),
            ("CMenuWorship_AddUsages", "Add Songs to Usages", "CMenuWorship_AddUsages_Click", "")
        };

        foreach (var (name, header, click, command) in expected)
        {
            var menuItem = composite.Descendants().Single(
                e => e.Name.LocalName == "MenuItem" && Attr(e, "Name") == name);
            Attr(menuItem, "Header").Should().Be(header);
            Attr(menuItem, "Tag").Should().Be(name);
            if (!string.IsNullOrEmpty(click))
            {
                Attr(menuItem, "Click").Should().Be(click);
            }

            if (!string.IsNullOrEmpty(command))
            {
                Attr(menuItem, "Command").Should().Contain(command);
            }
        }

        LoadText("Easislides.Wpf/Composites/WorshipListPanel.xaml.cs")
            .Should().Contain("QueueList.SelectAll()")
            .And.Contain("QueueList.SelectedItems.Clear()",
                "FrmMain CMenuWorship Select All/Unselect All should change the lower-left Worship List selection")
            .And.Contain("private void WL_Word_Click",
                "WL_Word should have a direct lower-left toolbar click handler")
            .And.Contain("new WorshipListRtfExporter()",
                "WL_Word should generate an actual RTF document instead of remaining a shell button")
            .And.Contain("public event RoutedEventHandler? EditSelectedItemRequested",
                "Edit item should route to MainWindow, which owns modal window lifetime")
            .And.Contain("CMenuWorship_Edit.IsEnabled = DataContext is MainViewModel editViewModel",
                "Edit item should be enabled only for editable DB song queue rows")
            .And.Contain("EditSelectedItemRequested?.Invoke(this, e)",
                "the panel should not construct SongEditorWindow directly")
            .And.Contain("CMenuWorship_AddUsages.IsEnabled = viewModel.CanAddWorshipListSongsToUsages",
                "Add Songs to Usages should be enabled when the queue contains DB song rows")
            .And.Contain("AddWorshipListSongsToUsagesAsync()",
                "the lower-left context menu should invoke the FrmMain AddToUsages parity path")
            .And.Contain("QueueList_PreviewMouseRightButtonDown",
                "right-click commands should target the row under the pointer");
        composite.ToString().Should().Contain("PlaySelectedWorshipMediaOnOutputCommand",
            "FrmMain CMenuWorship_PlayOnOutput should be visible from the lower-left Worship List context menu");
    }

    [Fact]
    public void MainWindow_Hosts_WorshipPanel_AlwaysVisible_BelowBrowserTabs_InLeftColumn()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var host = window.Descendants().SingleOrDefault(e => e.Name.LocalName == "WorshipListPanel");
        host.Should().NotBeNull("MainWindow should host the WorshipListPanel composite");

        var leftTabs = window.Descendants().SingleOrDefault(
            e => e.Name.LocalName == "TabControl" && Attr(e, "Name") == "LeftBrowserTabs");
        leftTabs.Should().NotBeNull("the left upper browser tab control should remain visible");
        Attr(leftTabs!, "Grid.Row").Should().Be("0", "source browser stays in the upper-left row");
        Attr(leftTabs!, "TabStripPlacement").Should().Be("Bottom", "FrmMain keeps source tabs on the bottom edge");
        leftTabs!.Descendants().Any(e => e.Name.LocalName == "WorshipListPanel")
            .Should().BeFalse("Worship List is a lower-left list role, not a source-browser tab");

        var listTabs = window.Descendants().SingleOrDefault(
            e => e.Name.LocalName == "TabControl" && Attr(e, "Name") == "LeftListTabs");
        listTabs.Should().NotBeNull("FrmMain lower-left pane should host Worship List and Praise Book tabs");
        Attr(listTabs!, "Grid.Row").Should().Be("2", "lower-left list tabs stay in the left lower row");
        Attr(listTabs!, "TabStripPlacement").Should().Be("Bottom", "FrmMain keeps lower-left list tabs on the bottom edge");
        listTabs!.Descendants().Any(e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "WorshipList")
            .Should().BeTrue("the first lower-left role is Worship List");
        listTabs!.Descendants().Any(e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == "PraiseBook")
            .Should().BeTrue("the second lower-left role is Praise Book");
        listTabs!.Descendants().Any(e => e.Name.LocalName == "WorshipListPanel")
            .Should().BeTrue("WorshipListPanel is hosted inside the lower-left Worship List tab");
        Attr(host!, "AddSelectedSourceRequested").Should().Be("WorshipListPanel_AddSelectedSourceRequested",
            "WL_Add is a lower-left control, but MainWindow owns the active upper source tab");
        Attr(host!, "OpenSessionNotesRequested").Should().Be("WorshipListPanel_OpenSessionNotesRequested",
            "WL_Notes is a lower-left control, but MainWindow owns modal window launch/ownership");
        Attr(host!, "EditSelectedItemRequested").Should().Be("WorshipListPanel_EditSelectedItemRequested",
            "FrmMain CMenuWorship_Edit should open the item editor through the owning MainWindow");
        Attr(listTabs.Parent!, "Grid.Column").Should().Be("0", "lower-left tabs stay in the left column");

        Attr(host!, "DataContext").Should().BeEmpty("DataContext inheritance must keep MainViewModel bindings intact");
        window.Descendants().Any(e => e.Name.LocalName == "ListBox" && Attr(e, "ItemsSource").Contains("Queue"))
            .Should().BeFalse("the queue ListBox remains inside the composite");
    }

    [Theory]
    // 증분149/150 — 좌측 브라우저 탭(라이브러리/성경/검색)이 Fluent 아이콘 머리글 + 스크린리더용 탭 이름을 가진다(아이콘 업그레이드·a11y).
    [InlineData("Folders", "Folders")]
    [InlineData("Bibles", "Bibles")]
    [InlineData("ImagesSource", "Images")]
    [InlineData("DefaultSource", "Default")]
    [InlineData("Search", "검색")]
    public void LeftBrowserTabs_HaveIconHeaders_AndAccessibleNames(string tag, string accessibleName)
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var tab = window.Descendants().Single(e => e.Name.LocalName == "TabItem" && Attr(e, "Tag") == tag);
        // 헤더에 종류 아이콘(SymbolIcon)이 있어야 한다.
        tab.Descendants().Any(e => e.Name.LocalName == "SymbolIcon").Should().BeTrue($"{tag} 탭 머리글에 아이콘");
        // 스크린리더가 글리프 대신 탭 이름을 읽도록 AutomationProperties.Name 고정.
        Attr(tab, "AutomationProperties.Name").Should().Be(accessibleName, "탭 접근성 이름은 한글 라벨");
        // 머리글 텍스트도 보존(아이콘 옆 글자).
        tab.Descendants().Any(e => e.Name.LocalName == "TextBlock" && Attr(e, "Text") == accessibleName)
            .Should().BeTrue($"{tag} 탭 머리글 텍스트 보존");
    }

    [Theory]
    // 증분151 — 중앙 미리보기 탭(Preview/PowerPoint/Media)도 Fluent 아이콘 머리글로 통일(좌측 탭과 일관). Tag 없어 접근성 이름으로 식별.
    [InlineData("Preview")]
    [InlineData("PowerPoint")]
    [InlineData("Media")]
    public void CentralPreviewTabs_HaveIconHeaders(string label)
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var tab = window.Descendants().Single(
            e => e.Name.LocalName == "TabItem" && Attr(e, "AutomationProperties.Name") == label);
        tab.Descendants().Any(e => e.Name.LocalName == "SymbolIcon").Should().BeTrue($"{label} 탭 머리글에 아이콘");
        tab.Descendants().Any(e => e.Name.LocalName == "TextBlock" && Attr(e, "Text") == label)
            .Should().BeTrue($"{label} 탭 머리글 텍스트 보존");
    }

    [Fact]
    public void ValidationProblems_AreClickableButtons_BoundToSelectCommand()
    {
        // 증분156 — 검증 문제 목록의 각 항목이 클릭 가능한 버튼(EsButton.Link)이고, 누르면 그 항목을 예배 순서에서 선택하는 커맨드에 바인딩됐는지 검증.
        var composite = LoadXaml("Easislides.Wpf/Composites/WorshipListPanel.xaml");

        var problemList = composite.Descendants().Single(
            e => e.Name.LocalName == "ItemsControl" && Attr(e, "ItemsSource").Contains("WorshipListProblems"));
        var button = problemList.Descendants().Single(e => e.Name.LocalName == "Button");

        Attr(button, "Command").Should().Contain("SelectWorshipProblemItemCommand", "문제 클릭 → 항목 선택 커맨드");
        Attr(button, "CommandParameter").Should().Contain("Binding", "클릭한 문제(WorshipItemProblem)를 파라미터로 전달");
        Attr(button, "Style").Should().Contain("EsButton.Link", "평평한 링크 버튼 스타일 사용");
        // 스크린리더가 문제 메시지를 읽도록 접근성 이름을 메시지에 고정.
        Attr(button, "AutomationProperties.Name").Should().Contain("Message");
    }

    [Fact]
    public void EsButtonLink_StyleExists_FlatAndTransparent()
    {
        // 증분156 — 재사용 가능한 모던 링크 버튼 스타일(보더·배경 없는 텍스트 버튼)이 디자인 시스템에 정의됐는지 확인.
        var dict = LoadXaml("Easislides.Wpf/Controls/EsButton.xaml");
        var link = dict.Descendants().Single(
            e => e.Name.LocalName == "Style" && Attr(e, "Key") == "EsButton.Link");

        Attr(link, "TargetType").Should().Be("Button");
        // 평평함의 핵심: 배경 투명 + 보더 0.
        var setters = link.Elements().Where(e => e.Name.LocalName == "Setter").ToArray();
        setters.Should().Contain(s => Attr(s, "Property") == "Background" && Attr(s, "Value") == "Transparent");
        setters.Should().Contain(s => Attr(s, "Property") == "BorderThickness" && Attr(s, "Value") == "0");
    }

    [Fact]
    public void Drop_AcceptsInlineInfoScreenSelection_AndInsertsAtTarget()
    {
        var code = LoadText("Easislides.Wpf/Composites/WorshipListPanel.xaml.cs");

        code.Should().Contain("typeof(InfoScreenSelection)", "InfoScr source drags use a typed payload");
        code.Should().Contain("typeof(InfoScreenSelection[])", "InfoScr multi-select drags use a typed payload array");
        code.Should().Contain("AddTextItemRelativeTo(infoScreen.Text, infoScreen.Options, targetItem)",
            "InfoScreen drops should preserve the Worship List drop position");
        code.Should().Contain("foreach (var selectedInfoScreen in infoScreens)",
            "InfoScreen multi-select drops should insert every selected saved notice");
    }

    [Fact]
    public void Drop_AcceptsInlinePraiseBookEntry_AndInsertsAtTarget()
    {
        var code = LoadText("Easislides.Wpf/Composites/WorshipListPanel.xaml.cs");

        code.Should().Contain("typeof(PraiseBookIndexEntry)", "PraiseBookItems drags use a typed payload");
        code.Should().Contain("AddPraiseBookSongRelativeToAsync(praiseBookEntry, targetItem)",
            "PraiseBook drops should reuse song resolution while preserving the Worship List drop position");
    }

    [Fact]
    public void SessionCombo_LoadsSelectedList_WithSelectionEnterAndDoubleClickGestures()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/WorshipListPanel.xaml");
        var code = LoadText("Easislides.Wpf/Composites/WorshipListPanel.xaml.cs");

        var combo = composite.Descendants().Single(
            e => e.Name.LocalName == "ComboBox" && Attr(e, "Name") == "SessionCombo");

        Attr(combo, "SelectionChanged").Should().Be("SessionCombo_SelectionChanged");
        Attr(combo, "KeyDown").Should().Be("SessionCombo_KeyDown");
        Attr(combo, "MouseDoubleClick").Should().Be("SessionCombo_MouseDoubleClick");
        code.Should().Contain("private bool _refreshingSessionCombo;");
        code.Should().Contain("private void SessionCombo_SelectionChanged");
        code.Should().Contain("sender is not ComboBox { IsKeyboardFocusWithin: true }",
            "initial binding and refresh-driven selection changes must not replace the current Worship List");
        code.Should().Contain("private bool TryLoadSelectedWorshipList()");
        code.Should().Contain("LoadSelectedWorshipListCommand.Execute(null)",
            "selection/Enter/double-click must reuse the same selected-list load path as the explicit button");
    }

    [Fact]
    public void WlOpen_AddsExternalFiles_FromLowerLeftToolbar()
    {
        var code = LoadText("Easislides.Wpf/Composites/WorshipListPanel.xaml.cs");

        code.Should().Contain("private async void WL_Open_Click",
            "FrmMain WL_Open should be implemented in the lower-left Worship List panel");
        code.Should().Contain("*.ppt;*.pptx;*.doc;*.docx;*.txt;*.esi;*.esw",
            "WL_Open should expose the legacy external-file filter family");
        code.Should().Contain("Multiselect = true",
            "the toolbar path should support adding several external files in one operation");
        code.Should().Contain("AddExternalFiles(new[] { fileName })",
            "PPT, media, and .esw files should use the same tested insertion path in the chosen file order");
        code.Should().Contain("AddWordTextItem(text)",
            "Word documents should become text/notice queue items like the existing menu path");
        code.Should().Contain("ExtractLegacyInfoScreenText",
            "legacy .esi files should be accepted as InfoScreen text rather than silently skipped");
    }

    [Fact]
    public void WlNotes_RoutesToSessionNotesWindow_FromLowerLeftToolbar()
    {
        var panelCode = LoadText("Easislides.Wpf/Composites/WorshipListPanel.xaml.cs");
        var windowCode = LoadText("Easislides.Wpf/MainWindow.xaml.cs");

        panelCode.Should().Contain("public event RoutedEventHandler? OpenSessionNotesRequested",
            "WL_Notes should raise an event to the owning MainWindow");
        panelCode.Should().Contain("private void WL_Notes_Click",
            "WL_Notes should have a direct lower-left toolbar click handler");
        panelCode.Should().Contain("OpenSessionNotesRequested?.Invoke(this, e)",
            "the panel should not construct the modal directly; MainWindow owns the current session and Owner window");
        windowCode.Should().Contain("private void WorshipListPanel_OpenSessionNotesRequested",
            "MainWindow should receive the lower-left WL_Notes event");
        windowCode.Should().Contain("=> OpenSessionNotes_Click(sender, e)",
            "menu and lower-left WL_Notes button should reuse the same session-notes window path");
        windowCode.Should().Contain("new Easislides.Wpf.Shell.WorshipSessionNotesViewModel",
            "the shared path should create the existing session notes VM");
    }

    [Fact]
    public void WlEdit_RoutesSelectedDbSong_ToSongEditorWindow()
    {
        var panelCode = LoadText("Easislides.Wpf/Composites/WorshipListPanel.xaml.cs");
        var windowCode = LoadText("Easislides.Wpf/MainWindow.xaml.cs");
        var editorCode = LoadText("Easislides.Wpf/Shell/WorshipTextItemEditorWindow.cs");

        panelCode.Should().Contain("private void CMenuWorship_Edit_Click",
            "FrmMain CMenuWorship_Edit should have a lower-left context menu click handler");
        panelCode.Should().Contain("CanEditSelectedWorshipItem",
            "DB song rows plus Bible/Notice text rows should enable the edit path");
        panelCode.Should().Contain("CanEditTextQueueItem",
            "Bible/Notice rows should no longer be disabled by the Worship List context menu");
        panelCode.Should().Contain("viewModel.SelectedItem = item",
            "right-clicking a different row should retarget the selected Worship List item before Edit runs");

        windowCode.Should().Contain("private async void WorshipListPanel_EditSelectedItemRequested",
            "MainWindow should receive the lower-left edit request");
        windowCode.Should().Contain("OpenSelectedWorshipItemEditorAsync",
            "the handler should choose the correct editor for the selected Worship row");
        windowCode.Should().Contain("OpenSelectedWorshipSongEditorAsync",
            "DB songs should still use the dedicated selected-Worship-song editor path");
        windowCode.Should().Contain("GetRequiredService<SongEditorWindow>()",
            "Edit item should reuse the WPF song editor instead of a placeholder");
        windowCode.Should().Contain("editorViewModel.LoadAsync(databasePath, folder, song)",
            "the editor should be loaded with the actual AdminDB song detail");
        windowCode.Should().Contain("viewModel.UpdateSelectedSongQueueItem(updatedSong, editorViewModel.Sequence, editorViewModel.FormatData)",
            "saving should refresh the selected queue row rather than leaving stale lyrics/title in Worship List");
        windowCode.Should().Contain("WorshipTextItemEditorWindow",
            "Bible/Notice rows should open an inline text editor instead of being rejected");
        windowCode.Should().Contain("viewModel.UpdateSelectedTextQueueItem",
            "saving Bible/Notice edits should replace the selected queue row");
        editorCode.Should().Contain("Edit Worship List Item")
            .And.Contain("ItemTitle")
            .And.Contain("ItemText",
                "the text editor should expose editable title/body values back to MainWindow");
    }

    [Theory]
    // 증분152 — 재정렬/복제 버튼의 접근성 이름이 카탈로그의 "실제" 단축키를 그대로 노출(메뉴·팔레트·Del 버튼과 같은 발견성 패턴).
    // 누군가 카탈로그 단축키를 바꾸면 ShortcutHint 가 달라져 이 테스트가 깨진다 → 버튼 힌트가 거짓이 되는 드리프트를 막는다.
    [InlineData("MoveSelectedItemUpCommand", MainCommandIds.WorshipMoveItemUp)]
    [InlineData("MoveSelectedItemDownCommand", MainCommandIds.WorshipMoveItemDown)]
    [InlineData("DuplicateSelectedItemCommand", MainCommandIds.WorshipDuplicateItem)]
    public void ReorderAndDuplicateButtons_AccessibleName_CarriesRealCatalogShortcut(string commandBinding, string commandId)
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/WorshipListPanel.xaml");

        // 카탈로그의 기본 단축키 표시문자열(예: "Ctrl+Shift+Up", "Ctrl+D") — 버튼 힌트의 정답.
        var hint = new CommandCatalog().All.Single(c => c.Id == commandId).ShortcutHint;
        hint.Should().NotBeEmpty("이 명령에 기본 단축키가 있어야 버튼 힌트가 의미를 가진다");

        var button = composite.Descendants().Single(
            e => e.Name.LocalName == "Button" && Attr(e, "Command").Contains(commandBinding));
        // 스크린리더가 읽는 접근성 이름이 실제 단축키를 포함해야 한다(글리프 ↑/↓ 대신 카탈로그와 일치하는 Up/Down 표기).
        Attr(button, "AutomationProperties.Name").Should().Contain(hint,
            "버튼 접근성 이름이 카탈로그 실제 단축키를 노출해야 한다(드리프트 방지)");
        // 마우스 사용자용 툴팁도 채워져 있어야 한다(발견성).
        Attr(button, "ToolTip").Should().NotBeEmpty("재정렬/복제 버튼은 호버 툴팁을 가진다");
    }
}
