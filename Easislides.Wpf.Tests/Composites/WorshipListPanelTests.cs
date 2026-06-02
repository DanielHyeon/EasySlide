using System.IO;
using System.Linq;
using System.Xml.Linq;
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

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    [Fact]
    public void Composite_Retains_Queue_ListBox_Binding_Surface()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/WorshipListPanel.xaml");

        var list = composite.Descendants().Single(e => e.Name.LocalName == "ListBox");
        Attr(list, "ItemsSource").Should().Contain("Queue");
        Attr(list, "SelectedItem").Should().Contain("SelectedItem").And.Contain("TwoWay");
        Attr(list, "AutomationProperties.Name").Should().Be("예배 순서 목록");

        // 증분137 — 항목 한 줄은 종류 아이콘 + 제목(DisplayMemberPath 대신 ItemTemplate). 종류 아이콘은 Kind→Fluent 변환기로 바인딩.
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
    }

    [Fact]
    public void MainWindow_Hosts_WorshipPanel_AlwaysVisible_BelowBrowserTabs_InLeftColumn()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var host = window.Descendants().SingleOrDefault(e => e.Name.LocalName == "WorshipListPanel");
        host.Should().NotBeNull("MainWindow 는 WorshipListPanel 컴포지트를 호스트해야 함");

        // FrmMain식 멀티페인(§7.4): 좌측 column 0 은 [브라우저 탭(라이브러리/성경)] 위 + [예배 순서] 아래로 분리되어
        // 예배 순서가 탭 전환 없이 "항상" 보인다. 즉 컴포지트는 더 이상 탭 안이 아니라 브라우저 탭과 형제(하단 Row 2).
        var leftTabs = window.Descendants().SingleOrDefault(
            e => e.Name.LocalName == "TabControl" && Attr(e, "Name") == "LeftBrowserTabs");
        leftTabs.Should().NotBeNull("좌측 상단은 라이브러리/성경 브라우저 탭 컨트롤");
        Attr(leftTabs!, "Grid.Row").Should().Be("0", "브라우저 탭은 좌측 상단(Row 0)");

        // 예배 순서는 탭 컨트롤 바깥(형제)에 위치 — 항상 표시. 탭 안에 있으면 탭 전환 시 가려진다.
        leftTabs!.Descendants().Any(e => e.Name.LocalName == "WorshipListPanel")
            .Should().BeFalse("예배 순서는 브라우저 탭 안이 아니라 항상 보이는 하단 패널");
        Attr(host!, "Grid.Row").Should().Be("2", "예배 순서는 좌측 하단(Row 2)에 항상 표시");

        // 컴포지트와 브라우저 탭은 같은 부모(좌측 column 0 Grid)를 공유한다.
        host!.Parent.Should().BeSameAs(leftTabs!.Parent, "둘 다 좌측 컬럼 Grid 의 자식");
        Attr(host!.Parent!, "Grid.Column").Should().Be("0", "좌측 컬럼(column 0)에 위치");

        // 라이브 경로 핵심 가드: 호스트에 DataContext 재정의가 없어야 MainViewModel 이 상속되어
        // Queue/SelectedItem 바인딩이 끊기지 않는다(LiveBar 처럼 DataContext 를 가로채면 라이브 선택 사고).
        Attr(host!, "DataContext").Should().BeEmpty("DataContext 상속이 끊기면 라이브 송출 선택이 끊김");

        // 송출 큐 ListBox 는 더 이상 윈도우에 인라인되지 않는다(중복 바인딩 방지).
        window.Descendants().Any(e => e.Name.LocalName == "ListBox" && Attr(e, "ItemsSource").Contains("Queue"))
            .Should().BeFalse("송출 큐 ListBox 는 컴포지트로 이동");
    }

    [Theory]
    // 증분149/150 — 좌측 브라우저 탭(라이브러리/성경/검색)이 Fluent 아이콘 머리글 + 스크린리더용 탭 이름을 가진다(아이콘 업그레이드·a11y).
    [InlineData("Library", "라이브러리")]
    [InlineData("Bible", "성경")]
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
}
