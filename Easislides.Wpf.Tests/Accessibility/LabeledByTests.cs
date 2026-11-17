using System.Linq;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Accessibility;

/// <summary>
/// 입력 필드 라벨 연결(LabeledBy) 정밀화 가드 — 계획서 §7.3 (작업 D1).
///
/// - 보이는 캡션이 있는 입력 필드는 캡션을 Name 에 베껴 넣지 말고
///   AutomationProperties.LabeledBy 로 라벨을 가리켜 단일 출처로 만든다.
/// - LabeledBy 는 같은 파일의 "텍스트 있는" 요소를 정확히 가리켜야 한다.
/// </summary>
public class LabeledByTests
{
    [Fact]
    public void Labeled_Inputs_Use_LabeledBy_Not_Duplicated_Name()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var files = XamlAccessibilityScanner.ProductionXamlFiles(repoRoot);
        var violations = LabeledByScanner.FindRedundantNames(files, repoRoot);

        var report = string.Join(
            "\n",
            violations
                .OrderBy(v => v.RelativePath)
                .ThenBy(v => v.Line)
                .Select(v => $"  {v.RelativePath}:{v.Line} <{v.Control}> — {v.Detail}"));

        violations.Should().BeEmpty(
            "보이는 캡션과 똑같은 Name 은 LabeledBy 로 대체해 단일 출처로 만들어야 합니다 (계획서 §7.3).\n" + report);
    }

    [Fact]
    public void LabeledBy_References_Resolve_To_Named_Text_Elements()
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        var files = XamlAccessibilityScanner.ProductionXamlFiles(repoRoot);
        var violations = LabeledByScanner.FindBrokenLabeledBy(files, repoRoot);

        var report = string.Join(
            "\n",
            violations
                .OrderBy(v => v.RelativePath)
                .ThenBy(v => v.Line)
                .Select(v => $"  {v.RelativePath}:{v.Line} <{v.Control}> — {v.Detail}"));

        violations.Should().BeEmpty(
            "AutomationProperties.LabeledBy 는 같은 파일의 텍스트 있는 요소를 정확히 가리켜야 합니다.\n" + report);
    }
}
