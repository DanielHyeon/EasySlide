using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Accessibility;

/// <summary>
/// 템플릿 파트 자동화 트리 노출 정책 검증 — 계획서 §7.3 (작업 D3).
///
/// Controls/ 의 ControlTemplate 내부 파트 정책:
///  - 구조 파트(x:Name="PART_*" 또는 Focusable="False")는 소유 컨트롤이 의미를 제공하므로 면제.
///  - 그 외 "포커스 가능한 독립 동작" 인터랙티브 파트는 반드시 접근성 이름을 가져야 한다.
///
/// 정책 로직을 합성 XAML(양성/음성)로 직접 검증하고, 실제 Controls/ 가 정책을
/// 위반하지 않음을 저장소 단위로 가드한다.
/// </summary>
public class TemplatePartsTests
{
    private static readonly XNamespace P = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static XDocument Parse(string innerXaml)
        => XDocument.Parse(
            $"<ResourceDictionary xmlns=\"{P}\" xmlns:x=\"{X}\">{innerXaml}</ResourceDictionary>");

    [Fact]
    public void Focusable_Unnamed_Interactive_Part_Is_Flagged()
    {
        // 포커스 가능 + 이름 없음 + PART_ 아님 → 독립 동작인데 이름이 없으므로 위반.
        var doc = Parse("<ControlTemplate><Button x:Name=\"CustomActionButton\" /></ControlTemplate>");

        var violations = XamlAccessibilityScanner.ScanDocument(doc, "Controls/Synthetic.xaml");

        violations.Should().ContainSingle()
            .Which.Control.Should().Be("Button");
    }

    [Fact]
    public void PART_Named_Part_Is_Exempt()
    {
        // WPF 명명 규약 파트 — 포커스 가능해도 소유 컨트롤이 이름을 제공하므로 면제.
        var doc = Parse("<ControlTemplate><TextBox x:Name=\"PART_EditableTextBox\" Focusable=\"True\" /></ControlTemplate>");

        XamlAccessibilityScanner.ScanDocument(doc, "Controls/Synthetic.xaml")
            .Should().BeEmpty();
    }

    [Fact]
    public void NonFocusable_Structural_Part_Is_Exempt()
    {
        // Focusable=False — 탭 순서에 없는 구조 파트(예: ComboBox 드롭다운 토글). 면제.
        var doc = Parse("<ControlTemplate><ToggleButton x:Name=\"ToggleButton\" Focusable=\"False\" /></ControlTemplate>");

        XamlAccessibilityScanner.ScanDocument(doc, "Controls/Synthetic.xaml")
            .Should().BeEmpty();
    }

    [Fact]
    public void Named_Interactive_Part_Is_Exempt()
    {
        // 독립 동작이지만 이름이 있으면 OK (예: CommandBar 오버플로 버튼 "추가 명령").
        var doc = Parse("<ControlTemplate><ToggleButton AutomationProperties.Name=\"추가 명령\" /></ControlTemplate>");

        XamlAccessibilityScanner.ScanDocument(doc, "Controls/Synthetic.xaml")
            .Should().BeEmpty();
    }

    [Fact]
    public void Production_Controls_Templates_Satisfy_Policy()
    {
        // 실제 Controls/ 템플릿이 정책을 위반하지 않아야 한다(향후 회귀 가드).
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var controlsFiles = XamlAccessibilityScanner.ProductionXamlFiles(repoRoot)
            .Where(p => p.Replace('\\', '/').Contains("/Controls/"))
            .ToList();

        controlsFiles.Should().NotBeEmpty("Controls/ 템플릿 파일이 스캔 대상에 포함되어야 함");

        var violations = XamlAccessibilityScanner.Scan(controlsFiles, repoRoot);

        var report = string.Join(
            "\n",
            violations.Select(v => $"  {v.RelativePath}:{v.Line} <{v.Control}>"));

        violations.Should().BeEmpty(
            "Controls/ 템플릿의 포커스 가능 인터랙티브 파트는 이름을 가져야 합니다 (계획서 §7.3 / D3).\n" + report);
    }
}
