using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Easislides.Wpf.Tests.Accessibility;

/// <summary>
/// XAML 파일을 스캔해 "접근성 이름(accessible name)이 없는 인터랙티브 컨트롤"을 찾아낸다.
/// 계획서 §7.3 — 모든 컨트롤에 이름, 아이콘-온리 버튼·입력 필드에 이름/라벨 필수.
///
/// 이름 출처로 인정하는 것:
///  - AutomationProperties.Name / AutomationProperties.LabeledBy 특성
///  - 텍스트가 보이는 Content/Header 특성 또는 자식 텍스트(TextBlock/Run/AccessText 포함)
///  - ToolTip 텍스트 (약하지만 이름 단서로 인정 — 오탐 최소화)
///
/// 스캐너는 휴리스틱이다(완벽한 XAML 의미 분석 아님). 목적은 "이름 단서가 전혀 없는" 컨트롤을 잡는 것.
/// </summary>
internal static class XamlAccessibilityScanner
{
    // 접근성 이름이 필요한 인터랙티브 컨트롤 (local name 기준).
    private static readonly HashSet<string> InteractiveControls = new(StringComparer.Ordinal)
    {
        "Button", "ToggleButton", "RepeatButton",
        "CheckBox", "RadioButton",
        "TextBox", "PasswordBox", "ComboBox",
        "ListBox", "ListView", "DataGrid", "Slider",
    };

    // XAML 디렉티브 네임스페이스 — x:Name 정확 판별용.
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    public readonly record struct Violation(string RelativePath, string Control, int Line);

    /// <summary>지정한 XAML 파일들에서 이름 없는 인터랙티브 컨트롤을 모은다.</summary>
    public static IReadOnlyList<Violation> Scan(IEnumerable<string> xamlFiles, string repoRoot)
    {
        var violations = new List<Violation>();

        foreach (var file in xamlFiles)
        {
            XDocument document;
            try
            {
                document = XDocument.Load(file, LoadOptions.SetLineInfo);
            }
            catch
            {
                continue; // 파싱 불가 파일은 건너뜀
            }

            var rel = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');
            violations.AddRange(ScanDocument(document, rel));
        }

        return violations;
    }

    /// <summary>
    /// 단일 XAML 문서에서 이름 없는 인터랙티브 컨트롤을 찾는다.
    /// (파일 기반 Scan 과 정책 단위 테스트가 공유 — 합성 XAML 검증 가능)
    /// </summary>
    public static IReadOnlyList<Violation> ScanDocument(XDocument document, string relativePath)
    {
        var violations = new List<Violation>();

        foreach (var element in document.Descendants())
        {
            if (!InteractiveControls.Contains(element.Name.LocalName))
            {
                continue;
            }

            // 템플릿 구조 파트(PART_*, Focusable=False)는 소유 컨트롤이 의미를 제공하므로 면제.
            if (IsExemptTemplatePart(element))
            {
                continue;
            }

            if (HasAccessibleName(element))
            {
                continue;
            }

            var line = (element as System.Xml.IXmlLineInfo)?.LineNumber ?? 0;
            violations.Add(new Violation(relativePath, element.Name.LocalName, line));
        }

        return violations;
    }

    /// <summary>
    /// 템플릿 구조 파트인지 — 자동화 트리에서 독립 이름이 필요 없는 경우.
    ///
    /// 정책 (계획서 §7.3, 작업 D3):
    ///  - WPF 명명 규약 파트(x:Name="PART_*"): 의미·이름을 소유 ControlTemplate 컨트롤이 제공.
    ///  - Focusable="False": 키보드 탭 순서에 없는 구조/장식 파트(예: ComboBox 드롭다운 토글) —
    ///    소유 컨트롤의 자동화 패턴(ExpandCollapse 등)이 의미를 제공.
    /// 그 외 "포커스 가능한 독립 동작" 파트(예: CommandBar 오버플로 버튼)는 면제 대상이 아니며
    /// 반드시 이름을 가져야 한다.
    /// </summary>
    private static bool IsExemptTemplatePart(XElement element)
    {
        var name = element.Attribute(XamlNamespace + "Name")?.Value
                   ?? element.Attribute("Name")?.Value;
        if (name is not null && name.StartsWith("PART_", StringComparison.Ordinal))
        {
            return true;
        }

        var focusable = element.Attribute("Focusable")?.Value;
        return string.Equals(focusable, "False", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasAccessibleName(XElement element)
    {
        // 1) AutomationProperties.Name / LabeledBy 특성
        foreach (var attribute in element.Attributes())
        {
            var local = attribute.Name.LocalName;
            if (local is "AutomationProperties.Name" or "AutomationProperties.LabeledBy"
                && !string.IsNullOrWhiteSpace(attribute.Value))
            {
                return true;
            }

            // 2) 보이는 텍스트 Content/Header/ToolTip 특성
            if (local is "Content" or "Header" or "ToolTip" && ContainsLetterOrDigit(attribute.Value))
            {
                return true;
            }
        }

        // 3) 자식 요소 안의 텍스트 (직접 inner text, 또는 TextBlock/Run/AccessText 의 Text/inner text)
        foreach (var node in element.DescendantNodes().OfType<XText>())
        {
            if (ContainsLetterOrDigit(node.Value))
            {
                return true;
            }
        }

        foreach (var descendant in element.Descendants())
        {
            if (descendant.Name.LocalName is "TextBlock" or "Run" or "AccessText" or "Label")
            {
                var text = descendant.Attribute("Text")?.Value ?? descendant.Attribute("Content")?.Value;
                if (ContainsLetterOrDigit(text))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool ContainsLetterOrDigit(string? value)
        => !string.IsNullOrEmpty(value) && value!.Any(char.IsLetterOrDigit);

    /// <summary>테스트 실행 위치에서 위로 올라가 저장소 루트(Easislides.sln 포함)를 찾는다.</summary>
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Easislides.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Easislides.sln 을 찾지 못해 저장소 루트를 결정할 수 없습니다.");
    }

    /// <summary>
    /// 접근성 스캔 대상 프로덕션 WPF XAML 파일 목록.
    /// 제외: bin/obj(빌드 산출물), Demo/Poc(개발용), App.xaml(부트스트랩).
    ///
    /// Controls/(스타일·ControlTemplate 정의)도 포함한다 — 단, 템플릿 내부 구조 파트는
    /// <see cref="IsExemptTemplatePart"/> 정책(PART_* / Focusable=False)으로 면제된다.
    /// 이렇게 하면 향후 커스텀 컨트롤 템플릿에 "이름 없는 포커스 가능 인터랙티브 파트"가
    /// 새로 들어올 때 가드가 잡아낸다(과거의 블랭킷 제외 → 원칙 기반 정책으로 정형화).
    /// </summary>
    public static IReadOnlyList<string> ProductionXamlFiles(string repoRoot)
    {
        var wpfRoot = Path.Combine(repoRoot, "Easislides.Wpf");
        return Directory.EnumerateFiles(wpfRoot, "*.xaml", SearchOption.AllDirectories)
            .Where(p =>
            {
                var rel = Path.GetRelativePath(repoRoot, p).Replace('\\', '/');
                return !rel.Contains("/bin/") && !rel.Contains("/obj/")
                    && !rel.Contains("/Demo/") && !rel.Contains("/Poc/")
                    && !rel.EndsWith("App.xaml", StringComparison.Ordinal);
            })
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
    }
}
