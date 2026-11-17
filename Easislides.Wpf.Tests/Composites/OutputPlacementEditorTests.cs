using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// OutputPlacementEditor 컴포지트 추출 동등성 가드 — 계획서 §5.2 / 작업 A-2.
///
/// SettingsWindow "송출" 탭의 출력 배치 영역(보조 모니터 우선 + 수동 위/왼쪽/폭)을
/// 재사용 UserControl로 분리했다. "View만 분리, ViewModel·바인딩 동일" 원칙.
///
///  1) 분리된 컴포지트가 원래 인라인 바인딩 면(4종)을 요소 단위로 그대로 보유한다.
///  2) SettingsWindow 는 더 이상 그 바인딩을 직접 인라인하지 않고 컴포지트를 호스트한다.
///
/// ViewModel(SettingsWindowViewModel)은 변경하지 않았으므로 동작 동등성은 기존
/// SettingsWindowViewModelTests 가 계속 보장한다(이 테스트는 View 이동 면만 고정).
/// (컨트롤 스타일 사전 미로드로 런타임 인스턴스화가 불가하므로 XAML 구조로 검증.)
/// </summary>
public class OutputPlacementEditorTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    private static XElement ElementByAutomationName(XDocument doc, string name)
        => doc.Descendants().Single(e => e.Attributes()
            .Any(a => a.Name.LocalName == "AutomationProperties.Name" && a.Value == name));

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    private static bool HasAttributeValueContaining(XDocument doc, string localName, string needle)
        => doc.Descendants().Any(e => e.Attributes()
            .Any(a => a.Name.LocalName == localName && a.Value.Contains(needle)));

    [Fact]
    public void Composite_Retains_Placement_Binding_Surface()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/OutputPlacementEditor.xaml");

        // 보조 모니터 우선 CheckBox — 바인딩 + Content + 터치 타깃 높이까지 보존.
        var secondary = ElementByAutomationName(composite, "보조 모니터 우선 사용");
        secondary.Name.LocalName.Should().Be("CheckBox");
        Attr(secondary, "IsChecked").Should().Contain("DisplayAlwaysUseSecondaryMonitor");
        Attr(secondary, "Content").Should().Be("보조 모니터 우선 사용");
        Attr(secondary, "MinHeight").Should().Contain("Spacing.TargetMin");

        // 수동 위치 3종 — 경로 + UpdateSourceTrigger=PropertyChanged + 입력 스타일 보존(시각 동등).
        foreach (var (autoName, path) in new[]
                 {
                     ("출력 모니터 위", "DisplayCustomTop"),
                     ("출력 모니터 왼쪽", "DisplayCustomLeft"),
                     ("출력 모니터 폭", "DisplayCustomWidth"),
                 })
        {
            var box = ElementByAutomationName(composite, autoName);
            box.Name.LocalName.Should().Be("TextBox");
            Attr(box, "Text").Should().Contain(path).And.Contain("UpdateSourceTrigger=PropertyChanged");
            Attr(box, "Style").Should().Contain("EsTextBox.Default");
        }
    }

    [Fact]
    public void Composite_Preserves_Layout_Structure()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/OutputPlacementEditor.xaml");

        // 루트는 StackPanel(수직) — 의도치 않은 Background/Margin/가로 방향이 추가되지 않아야
        // 호스트(수직 StackPanel) 안에서 레이아웃이 인라인 시절과 동일하다.
        var root = composite.Root!.Elements().Single();
        root.Name.LocalName.Should().Be("StackPanel");
        Attr(root, "Background").Should().BeEmpty("래퍼에 배경이 추가되면 시각 회귀");
        Attr(root, "Margin").Should().BeEmpty("래퍼에 마진이 추가되면 간격 회귀");
        Attr(root, "Orientation").Should().NotBe("Horizontal", "수직 스택이어야 인라인 시절과 동일");

        // 수동 위치 Grid — 마진과 3등분 컬럼 보존.
        var grid = composite.Descendants().Single(e => e.Name.LocalName == "Grid");
        Attr(grid, "Margin").Should().Be("0,4,0,16");
        grid.Descendants().Count(e => e.Name.LocalName == "ColumnDefinition")
            .Should().Be(3, "위/왼쪽/폭 3등분 컬럼 보존");
        grid.Descendants()
            .Where(e => e.Name.LocalName == "ColumnDefinition")
            .Should().OnlyContain(c => Attr(c, "Width") == "*");
    }

    [Fact]
    public void Window_Hosts_Composite_And_No_Longer_Inlines_Placement()
    {
        var window = LoadXaml("Easislides.Wpf/SettingsWindow.xaml");

        var host = window.Descendants().SingleOrDefault(e => e.Name.LocalName == "OutputPlacementEditor");
        host.Should().NotBeNull("SettingsWindow 는 OutputPlacementEditor 컴포지트를 호스트해야 함");

        // 위치 보존 — 컴포지트는 "간격 화면 페이드 전환" CheckBox 바로 뒤에 와야 한다
        // (송출 탭 StackPanel 내 원래 자리). 위치가 틀어지면 레이아웃 회귀.
        var siblings = host!.Parent!.Elements().ToList();
        var index = siblings.IndexOf(host);
        index.Should().BeGreaterThan(0);
        var previous = siblings[index - 1];
        previous.Attributes().Any(a => a.Name.LocalName == "AutomationProperties.Name" && a.Value == "간격 화면 페이드 전환")
            .Should().BeTrue("컴포지트는 간격 화면 페이드 CheckBox 바로 뒤 원래 위치에 있어야 함");

        // 이동한 바인딩은 윈도우에 더 이상 인라인되지 않는다(중복 정의 방지).
        HasAttributeValueContaining(window, "IsChecked", "DisplayAlwaysUseSecondaryMonitor").Should().BeFalse("보조 모니터 바인딩은 컴포지트로 이동");
        HasAttributeValueContaining(window, "Text", "DisplayCustomTop").Should().BeFalse("출력 위 바인딩은 컴포지트로 이동");
        HasAttributeValueContaining(window, "Text", "DisplayCustomLeft").Should().BeFalse("출력 왼쪽 바인딩은 컴포지트로 이동");
        HasAttributeValueContaining(window, "Text", "DisplayCustomWidth").Should().BeFalse("출력 폭 바인딩은 컴포지트로 이동");
    }
}
