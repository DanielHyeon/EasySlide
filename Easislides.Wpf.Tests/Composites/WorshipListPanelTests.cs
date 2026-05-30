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
        Attr(list, "DisplayMemberPath").Should().Be("Title");
        Attr(list, "AutomationProperties.Name").Should().Be("예배 순서 목록");

        // 카드 제목 보존.
        composite.Descendants().Any(e => e.Name.LocalName == "TextBlock" && Attr(e, "Text") == "예배 순서")
            .Should().BeTrue("예배 순서 제목 보존");
    }

    [Fact]
    public void MainWindow_Hosts_Composite_In_Left_Column0_TabControl_And_No_Longer_Inlines_List()
    {
        var window = LoadXaml("Easislides.Wpf/MainWindow.xaml");

        var host = window.Descendants().SingleOrDefault(e => e.Name.LocalName == "WorshipListPanel");
        host.Should().NotBeNull("MainWindow 는 WorshipListPanel 컴포지트를 호스트해야 함");

        // §7.5 P0 인라인 브라우저: 좌측 column 0 은 [예배 순서]+[라이브러리] 탭 컨트롤이 차지하고,
        // 그 안의 "예배 순서" 탭에 컴포지트가 위치한다(예배 순서는 여전히 좌측 column 0 영역).
        var leftTabs = window.Descendants().SingleOrDefault(
            e => e.Name.LocalName == "TabControl" && Attr(e, "Name") == "LeftBrowserTabs");
        leftTabs.Should().NotBeNull("좌측 column 0 은 예배순서/라이브러리 탭 컨트롤");
        Attr(leftTabs!, "Grid.Column").Should().Be("0", "좌측 탭 컨트롤은 column 0 위치를 유지해야 함");
        leftTabs!.Descendants().Any(e => e.Name.LocalName == "WorshipListPanel")
            .Should().BeTrue("WorshipListPanel 은 좌측 탭 컨트롤 안에 위치");

        // 라이브 경로 핵심 가드: 호스트에 DataContext 재정의가 없어야 MainViewModel 이 상속되어
        // Queue/SelectedItem 바인딩이 끊기지 않는다(LiveBar 처럼 DataContext 를 가로채면 라이브 선택 사고).
        Attr(host!, "DataContext").Should().BeEmpty("DataContext 상속이 끊기면 라이브 송출 선택이 끊김");

        // 송출 큐 ListBox 는 더 이상 윈도우에 인라인되지 않는다(중복 바인딩 방지).
        window.Descendants().Any(e => e.Name.LocalName == "ListBox" && Attr(e, "ItemsSource").Contains("Queue"))
            .Should().BeFalse("송출 큐 ListBox 는 컴포지트로 이동");
    }
}
