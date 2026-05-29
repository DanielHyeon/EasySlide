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

            foreach (var element in document.Descendants())
            {
                if (!InteractiveControls.Contains(element.Name.LocalName))
                {
                    continue;
                }

                if (HasAccessibleName(element))
                {
                    continue;
                }

                var line = (element as System.Xml.IXmlLineInfo)?.LineNumber ?? 0;
                var rel = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');
                violations.Add(new Violation(rel, element.Name.LocalName, line));
            }
        }

        return violations;
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
    /// 제외: bin/obj(빌드 산출물), Demo/Poc(개발용), App.xaml(부트스트랩),
    ///       Controls/(스타일·ControlTemplate 정의 — 인스턴스 트리가 아니라 템플릿 내부 파트는
    ///       소유 컨트롤 인스턴스가 이름을 제공하므로 개별 이름 대상이 아님).
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
                    && !rel.Contains("/Controls/")
                    && !rel.EndsWith("App.xaml", StringComparison.Ordinal);
            })
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
    }
}
