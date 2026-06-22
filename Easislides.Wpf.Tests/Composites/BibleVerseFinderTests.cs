using System.IO;
using System.Linq;
using System.Xml.Linq;
using Easislides.Wpf.Tests.Accessibility;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Composites;

/// <summary>
/// BibleVerseFinder 컴포지트 추출 동등성 가드 — 계획서 §5.2 / 작업 A.
///
/// "View만 분리, ViewModel·바인딩 동일" 원칙의 동등성을 고정한다:
///  1) 분리된 컴포지트가 원래 인라인 바인딩 면(버전/책/검색/명령)을 그대로 보유한다.
///  2) BibleWindow 는 더 이상 그 바인딩을 직접 인라인하지 않고 컴포지트를 호스트한다.
///
/// ViewModel(BibleViewModel)은 변경하지 않았으므로 동작 동등성은 기존
/// BibleViewModelTests 가 계속 보장한다(이 테스트는 View 이동 면만 고정).
///
/// (컨트롤 스타일 사전이 테스트에 로드되지 않아 UserControl 런타임 인스턴스화가
///  불가하므로, 바인딩 면은 XAML 구조로 검증한다 — 기존 XAML 스캐너 패턴과 동일.)
/// </summary>
public class BibleVerseFinderTests
{
    private static XDocument LoadXaml(string relativePath)
    {
        var repoRoot = XamlAccessibilityScanner.FindRepoRoot();
        return XDocument.Load(Path.Combine(repoRoot, relativePath), LoadOptions.None);
    }

    /// <summary>지정한 로컬 속성명을 가진 값이 문서 어디든 존재하는지(바인딩 토큰 포함).</summary>
    private static bool HasAttributeValueContaining(XDocument doc, string localName, string needle)
        => doc.Descendants().Any(e => e.Attributes()
            .Any(a => a.Name.LocalName == localName && a.Value.Contains(needle)));

    /// <summary>AutomationProperties.Name 이 일치하는 단일 요소를 찾는다(없거나 중복이면 실패).</summary>
    private static XElement ElementByAutomationName(XDocument doc, string name)
        => doc.Descendants().Single(e => e.Attributes()
            .Any(a => a.Name.LocalName == "AutomationProperties.Name" && a.Value == name));

    private static string Attr(XElement e, string localName)
        => e.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value ?? string.Empty;

    [Fact]
    public void Composite_Retains_VerseFinder_Binding_Surface()
    {
        var composite = LoadXaml("Easislides.Wpf/Composites/BibleVerseFinder.xaml");

        // 요소 단위로 검증 — 바인딩 경로뿐 아니라 모드/트리거/표시경로/접근성 이름이 같은
        // 컨트롤 위에 그대로 보존됐는지까지 확인(느슨한 토큰 존재 검사로는 못 잡는 회귀 방지).

        // 버전 ComboBox
        var version = ElementByAutomationName(composite, "성경 버전");
        version.Name.LocalName.Should().Be("ComboBox");
        Attr(version, "ItemsSource").Should().Contain("Versions");
        Attr(version, "SelectedItem").Should().Contain("SelectedVersion").And.Contain("TwoWay");
        Attr(version, "DisplayMemberPath").Should().Be("Name");

        // 책 ComboBox
        var book = ElementByAutomationName(composite, "성경 책");
        book.Name.LocalName.Should().Be("ComboBox");
        Attr(book, "ItemsSource").Should().Contain("Books");
        Attr(book, "SelectedItem").Should().Contain("SelectedBook").And.Contain("TwoWay");
        Attr(book, "DisplayMemberPath").Should().Be("Name");

        // 검색어 TextBox — UpdateSourceTrigger=PropertyChanged 까지 보존되어야 즉시 검색 동작 동일.
        var search = ElementByAutomationName(composite, "성경 검색어");
        search.Name.LocalName.Should().Be("TextBox");
        Attr(search, "Text").Should().Contain("SearchText").And.Contain("UpdateSourceTrigger=PropertyChanged");

        // 불러오기 / 검색 명령 버튼
        var load = ElementByAutomationName(composite, "성경 책 불러오기");
        Attr(load, "Command").Should().Contain("LoadSelectedBookCommand");

        var searchBtn = ElementByAutomationName(composite, "성경 검색");
        Attr(searchBtn, "Command").Should().Contain("SearchCommand");
    }

    [Fact]
    public void Window_Hosts_Composite_And_No_Longer_Inlines_VerseFinder()
    {
        var window = LoadXaml("Easislides.Wpf/Library/BibleWindow.xaml");

        // 컴포지트를 호스트한다.
        window.Descendants().Any(e => e.Name.LocalName == "BibleVerseFinder")
            .Should().BeTrue("BibleWindow 는 BibleVerseFinder 컴포지트를 호스트해야 함");

        // 이동한 바인딩은 윈도우에 더 이상 인라인되지 않는다(중복 정의 방지).
        HasAttributeValueContaining(window, "SelectedItem", "SelectedVersion").Should().BeFalse("선택 버전 바인딩은 컴포지트로 이동");
        HasAttributeValueContaining(window, "SelectedItem", "SelectedBook").Should().BeFalse("선택 책 바인딩은 컴포지트로 이동");
        HasAttributeValueContaining(window, "Text", "SearchText").Should().BeFalse("검색어 바인딩은 컴포지트로 이동");
        HasAttributeValueContaining(window, "Command", "LoadSelectedBookCommand").Should().BeFalse("불러오기 명령은 컴포지트로 이동");
        HasAttributeValueContaining(window, "Command", "SearchCommand").Should().BeFalse("검색 명령은 컴포지트로 이동");
    }
}
