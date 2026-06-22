using System.Linq;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Accessibility;

/// <summary>
/// 접근성 가드 테스트 (계획서 §7.3, §9.2) — 이름 없는 인터랙티브 컨트롤을 차단한다.
///
/// 프로덕션 WPF XAML(Demo/Poc/Controls 제외)의 모든 인터랙티브 컨트롤은
/// 접근성 이름 출처(AutomationProperties.Name/LabeledBy, 텍스트 Content/Header/ToolTip,
/// 자식 텍스트)를 가져야 한다. 새 위반이 생기면 이 테스트가 실패한다.
/// </summary>
public class XamlAccessibilityTests
{
    [Fact]
    public void All_Interactive_Controls_Have_Accessible_Names()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var files = XamlAccessibilityScanner.ProductionXamlFiles(repoRoot);
        var violations = XamlAccessibilityScanner.Scan(files, repoRoot);

        var report = string.Join(
            "\n",
            violations
                .OrderBy(v => v.RelativePath)
                .ThenBy(v => v.Line)
                .Select(v => $"  {v.RelativePath}:{v.Line} <{v.Control}> — AutomationProperties.Name 누락"));

        violations.Should().BeEmpty(
            "모든 인터랙티브 컨트롤에는 접근성 이름이 있어야 합니다 (계획서 §7.3).\n" + report);
    }
}
