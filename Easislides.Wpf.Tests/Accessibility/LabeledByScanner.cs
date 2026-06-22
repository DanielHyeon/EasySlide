using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Easislides.Wpf.Tests.Accessibility;

/// <summary>
/// 입력 필드의 라벨 연결(AutomationProperties.LabeledBy) 정밀화 검사 — 계획서 §7.3 (작업 D1).
///
/// 두 가지를 검사한다:
///  1) 중복 Name 안티패턴 — 입력 필드(TextBox/PasswordBox/ComboBox)의
///     AutomationProperties.Name 이 같은 컨테이너 안의 캡션 TextBlock/Label 텍스트와
///     "똑같다"면, 이름이 두 곳(보이는 라벨 + Name)에 중복된 것이다.
///     → LabeledBy 로 라벨을 가리켜 단일 출처로 만들어야 한다.
///  2) 깨진 LabeledBy — AutomationProperties.LabeledBy="{Binding ElementName=X}" 가
///     같은 파일에 존재하지 않거나 텍스트가 없는 요소를 가리키면, 접근성 이름이
///     실제로는 비어버린다(기존 Name 가드는 LabeledBy 값만 보고 통과시켜 못 잡음).
///
/// 휴리스틱이지만 결정적이다(정확 텍스트 일치 + 같은 컨테이너). 목적은 "캡션을 Name에
/// 베껴 넣은" 정확한 중복만 LabeledBy 전환 대상으로 잡고, 모호한 경우는 건드리지 않는 것.
/// </summary>
internal static class LabeledByScanner
{
    // 캡션(보이는 라벨)으로 연결할 입력 컨트롤 — 인라인 텍스트가 없는 텍스트성 입력.
    private static readonly HashSet<string> LabelableInputs = new(StringComparer.Ordinal)
    {
        "TextBox", "PasswordBox", "ComboBox",
    };

    private const string NameAttr = "AutomationProperties.Name";
    private const string LabeledByAttr = "AutomationProperties.LabeledBy";

    // XAML 디렉티브 네임스페이스 — x:Name 을 정확히 판별하기 위해 (부분 문자열 매칭이 아니라).
    private static readonly XNamespace XamlNs = "http://schemas.microsoft.com/winfx/2006/xaml";

    public readonly record struct Violation(string RelativePath, string Control, int Line, string Detail);

    /// <summary>
    /// 같은 컨테이너의 캡션 텍스트와 정확히 같은 AutomationProperties.Name 을 가진 입력 필드
    /// (= LabeledBy 로 바꿔야 할 중복)를 찾는다.
    /// </summary>
    public static IReadOnlyList<Violation> FindRedundantNames(IEnumerable<string> xamlFiles, string repoRoot)
    {
        var violations = new List<Violation>();

        foreach (var file in xamlFiles)
        {
            var document = TryLoad(file);
            if (document is null) continue;

            foreach (var input in document.Descendants()
                         .Where(e => LabelableInputs.Contains(e.Name.LocalName)))
            {
                // 이미 LabeledBy 로 연결된 입력은 검사 대상 아님.
                if (HasAttr(input, LabeledByAttr)) continue;

                var name = GetAttr(input, NameAttr);
                if (string.IsNullOrWhiteSpace(name)) continue;

                // 같은 부모 컨테이너 안에서 캡션 텍스트가 Name 과 똑같은 TextBlock/Label 찾기.
                if (HasMatchingCaption(input, name!))
                {
                    var line = (input as System.Xml.IXmlLineInfo)?.LineNumber ?? 0;
                    var rel = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');
                    violations.Add(new Violation(rel, input.Name.LocalName, line,
                        $"AutomationProperties.Name=\"{name}\" 이 인접 캡션과 중복 — LabeledBy 로 연결 필요"));
                }
            }
        }

        return violations;
    }

    /// <summary>
    /// AutomationProperties.LabeledBy="{Binding ElementName=X}" 가 같은 파일의
    /// 텍스트 있는 x:Name="X" 요소를 가리키지 않는 "깨진" 연결을 찾는다.
    /// </summary>
    public static IReadOnlyList<Violation> FindBrokenLabeledBy(IEnumerable<string> xamlFiles, string repoRoot)
    {
        var violations = new List<Violation>();

        foreach (var file in xamlFiles)
        {
            var document = TryLoad(file);
            if (document is null) continue;

            // 파일 내 x:Name → 텍스트 보유 여부 맵.
            var namedWithText = document.Descendants()
                .Select(e => (Name: GetXName(e), HasText: HasVisibleText(e)))
                .Where(t => !string.IsNullOrEmpty(t.Name))
                .GroupBy(t => t.Name!, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Any(t => t.HasText), StringComparer.Ordinal);

            foreach (var element in document.Descendants())
            {
                var labeledBy = GetAttr(element, LabeledByAttr);
                if (string.IsNullOrWhiteSpace(labeledBy)) continue;

                var target = ExtractElementName(labeledBy!);
                var line = (element as System.Xml.IXmlLineInfo)?.LineNumber ?? 0;
                var rel = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');

                if (target is null)
                {
                    violations.Add(new Violation(rel, element.Name.LocalName, line,
                        $"LabeledBy=\"{labeledBy}\" 에서 ElementName 을 해석할 수 없음"));
                }
                else if (!namedWithText.TryGetValue(target, out var hasText))
                {
                    violations.Add(new Violation(rel, element.Name.LocalName, line,
                        $"LabeledBy 가 가리키는 x:Name=\"{target}\" 요소가 같은 파일에 없음"));
                }
                else if (!hasText)
                {
                    violations.Add(new Violation(rel, element.Name.LocalName, line,
                        $"LabeledBy 가 가리키는 \"{target}\" 에 보이는 텍스트가 없음(이름이 비어버림)"));
                }
            }
        }

        return violations;
    }

    private static bool HasMatchingCaption(XElement input, string name)
    {
        var parent = input.Parent;
        if (parent is null) return false;

        var target = name.Trim();

        // 직계 형제(Elements) 중 캡션(TextBlock/Label)으로, 정적 텍스트가 Name 과 정확히 일치하는 것의 개수.
        // - Descendants 가 아니라 Elements: 무관한 다른 그룹의 깊은 캡션 오탐 방지.
        // - 정확히 1개일 때만 "중복"으로 판정: 같은 텍스트 캡션이 여러 개면 어느 것이 이 입력의
        //   라벨인지 모호하므로 건드리지 않는다(설계 철학: 모호하면 스킵).
        var matchCount = parent.Elements()
            .Where(e => e.Name.LocalName is "TextBlock" or "Label")
            .Select(GetStaticText)
            .Count(text => text is not null && string.Equals(text, target, StringComparison.Ordinal));

        return matchCount == 1;
    }

    /// <summary>
    /// 요소의 "정적" 표시 텍스트를 추출 — Text/Content 특성 또는 내부 텍스트 노드.
    /// 바인딩식({Binding ...})·빈 문자열은 정적 캡션이 아니므로 null.
    /// </summary>
    private static string? GetStaticText(XElement e)
    {
        var attr = (GetAttr(e, "Text") ?? GetAttr(e, "Content"))?.Trim();
        if (!string.IsNullOrEmpty(attr) && !attr!.StartsWith("{", StringComparison.Ordinal))
        {
            return attr;
        }

        // 인라인 콘텐츠: <TextBlock>제목</TextBlock>
        var inner = string.Concat(e.Nodes().OfType<XText>().Select(t => t.Value)).Trim();
        return string.IsNullOrEmpty(inner) ? null : inner;
    }

    private static XDocument? TryLoad(string file)
    {
        try { return XDocument.Load(file, LoadOptions.SetLineInfo); }
        catch { return null; }
    }

    private static bool HasAttr(XElement e, string localName)
        => e.Attributes().Any(a => a.Name.LocalName == localName && !string.IsNullOrWhiteSpace(a.Value));

    private static string? GetAttr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value;

    private static string? GetXName(XElement e)
        // x:Name (XAML 디렉티브, 정석) 우선, 없으면 네임스페이스 없는 런타임 Name 특성.
        // 둘 다 ElementName 바인딩 대상이 된다. (느슨한 LocalName 매칭은 의도치 않은 속성을 잡으므로 금지)
        => e.Attribute(XamlNs + "Name")?.Value
           ?? e.Attribute("Name")?.Value;

    private static bool HasVisibleText(XElement e)
        => GetStaticText(e)?.Any(char.IsLetterOrDigit) ?? false;

    // "{Binding ElementName=Foo}", "{Binding ElementName = Foo, Mode=OneWay}" 형태.
    private static readonly Regex ElementNameRegex =
        new(@"ElementName\s*=\s*(?<name>[A-Za-z_][\w]*)", RegexOptions.Compiled);

    // "{x:Reference Foo}", "{x:Reference Name=Foo}" 형태.
    private static readonly Regex XReferenceRegex =
        new(@"x:Reference\s+(?:Name\s*=\s*)?(?<name>[A-Za-z_][\w]*)", RegexOptions.Compiled);

    /// <summary>"{Binding ElementName=Foo}" 또는 "{x:Reference Foo}"(Name= 형 포함)에서 Foo 추출.</summary>
    private static string? ExtractElementName(string markup)
    {
        var m = ElementNameRegex.Match(markup);
        if (m.Success) return m.Groups["name"].Value;

        m = XReferenceRegex.Match(markup);
        if (m.Success) return m.Groups["name"].Value;

        return null;
    }
}
